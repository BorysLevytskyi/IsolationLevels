using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Writer.DbProviders;

namespace Writer
{
    public static class DoubleBookingScenario
    {
        public static async Task Definition(IDatabaseProvider db, IsolationLevel isolationLevel)
        {
             await db.ResetDatabase();

            Console.WriteLine($"DoubleBookingScenario: {isolationLevel}");

            Task OnBeforeCommitOf(string actor) => db.Transaction(a => a.PrintBookings($"Before commit owner {actor}: "));

            ManualResetEventSlim evt = new ManualResetEventSlim(false);

            var t1 = db.Transaction(BookingFlow(evt, 1, "Borys", "12:00", "13:00", OnBeforeCommitOf), isolationLevel, actorName: "Borys");
            var t2 = db.Transaction(BookingFlow(evt, 2, "John", "12:59", "13:30", OnBeforeCommitOf), isolationLevel, actorName: "John");

            Console.WriteLine("Setting event...");
            evt.Set();

            await Task.WhenAll(t1, t2);

            Console.WriteLine();
            Console.WriteLine("Result:");
            await db.Transaction(u => u.PrintBookings("Final Read: "));
        }

        private static Func<Actor, Task> BookingFlow(
            ManualResetEventSlim start, 
            int roomId, 
            string owner, 
            string starTime, 
            string endTime, 
            Func<string, Task> onBeforeCommitOf, 
            TimeSpan? startDelay = null,
            string name = null)
        {
            return async actor =>
            {
                await Task.Yield(); // Important before waiting for an event

                start.Wait();

                try
                {
                    if (startDelay != null)
                    {
                        Thread.Sleep(startDelay.Value);
                    }

                    actor.Print("Check available bookings");

                    var o = await actor.GetExistingBookingOwnerIfAny(roomId, starTime, endTime);

                    if (o != null)
                    {
                        actor.Print($"Booking for #{roomId} room is owner by {o}. Cannot book.");
                        return;
                    }

                    actor.Print("Booking is availble");

                    await actor.Sleep(2.Sec());

                    await actor.BookRoom(roomId, starTime, endTime, owner);

                    actor.Print($"Room #{roomId} is booked");

                    await onBeforeCommitOf(owner);

                    await Task.Delay(2.Sec());

                    actor.Print("Commiting transaction...");
                
                    actor.CommitTransaction();

                    actor.Print("Success");
                }
                catch (Exception ex)
                {
                    actor.Print($"Aborted: {ex.Message}");
                }
            };
        }

        private static readonly object _lock = new object();
    }
}