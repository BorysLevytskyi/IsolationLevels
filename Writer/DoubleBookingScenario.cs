using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Writer.Postgres;

namespace Writer
{
    public static class DoubleBookingScenario
    {
        public static async Task Definition(SetupAction preStart, ExecuteAction execute, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
             await preStart();

            Console.WriteLine($"DoubleBookingScenario: {isolationLevel}");

            Task OnBeforeCommitOf(string actor) => execute(a => a.PrintBookings($"Before commit owner {actor}: "));

            ManualResetEventSlim evt = new ManualResetEventSlim(false);

            var t1 = execute(BookingRoutineFor(evt, 1, "Borys", "12:00", "13:00", OnBeforeCommitOf), isolationLevel);
            var t2 = execute(BookingRoutineFor(evt, 2, "John", "12:59", "13:30", OnBeforeCommitOf), isolationLevel);

            Console.WriteLine("Setting event...");
            evt.Set();

            await Task.WhenAll(t1, t2);

            Console.WriteLine();
            Console.WriteLine("Result:");
            await execute(u => u.PrintBookings("Final Read: "));
        }

        private static Func<Actor, Task> BookingRoutineFor(
            ManualResetEventSlim start, 
            int roomId, 
            string owner, 
            string starTime, 
            string endTime, 
            Func<string, Task> onBeforeCommitOf, 
            TimeSpan? startDelay = null)
        {
            return async actor =>
            {
                void Print(string message)
                {
                    lock (_lock)
                    {
                        Console.WriteLine($"{owner}: {message}");
                    }
                }

                await Task.Yield(); // Important before waiting for an event

                start.Wait();

                try
                {
                    if (startDelay != null)
                    {
                        Thread.Sleep(startDelay.Value);
                    }

                    Print("Check available bookings");

                    var o = await actor.GetExistingBookingOwnerIfAny(roomId, starTime, endTime);

                    if (o != null)
                    {
                        Print($"Booking for #{roomId} room is owner by {o}. Cannot book.");
                        return;
                    }

                    Print("Booking is availble");

                    await actor.Sleep(2.Sec());

                    await actor.BookRoom(roomId, starTime, endTime, owner, Print);

                    Print("Room is booked");

                    await onBeforeCommitOf(owner);

                    await Task.Delay(2.Sec());

                    Print("Commiting transaction...");
                
                    actor.CommitTransaction(Print);

                    Print("Success");
                }
                catch (Exception ex)
                {
                    Print($"Aborted: {ex.Message}");
                }
            };
        }

        private static readonly object _lock = new object();
    }
}