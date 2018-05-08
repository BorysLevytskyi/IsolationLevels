using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Db.IsolationLevels.Postgres;
using Npgsql;
using Console = System.Console;
using static Db.IsolationLevels.Times;
// ReSharper disable InconsistentNaming

namespace Db.IsolationLevels
{
    partial class Program
    {
        static object _lock = new object();

        static void Main(string[] args)
        {
            Console.WriteLine("Started");

            try
            {
                var isolationLevel = IsolationLevel.ReadCommitted;

                Console.WriteLine("------- MySql -----------------------------------");
                RunMysql(isolationLevel).GetAwaiter().GetResult();
                Console.WriteLine();
                Console.WriteLine("------ Postgres -----------------------------------");
                RunPostgres(isolationLevel).GetAwaiter().GetResult();

                Console.WriteLine("Done");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static async Task RunPostgres(IsolationLevel isolationLevel)
        {
            await RunDatabaseTest(PostgresDb.Seed, PostgresDb.Execute, isolationLevel);
        }

        public static async Task RunMysql(IsolationLevel isolationLevel)
        {
            await RunDatabaseTest(MySqlDb.Seed, MySqlDb.Execute, isolationLevel);
        }

        private static async Task RunDatabaseTest(Func<Task> seed, ExecuteDelegate execute, IsolationLevel isolationLevel)
        {
            await seed();


            Console.WriteLine($"Test: {isolationLevel}");

            Task OnBeforeCommitOf(string actor) => execute(a => a.PrintBookings($"Before commit owner {actor}: "));

            ManualResetEventSlim evt = new ManualResetEventSlim(false);

            var t1 = execute(BookingRoutineFor(evt, 1, "Borys", "12:00", "13:00", OnBeforeCommitOf), isolationLevel);
            var t2 = execute(BookingRoutineFor(evt, 2, "John", "12:59", "13:30", OnBeforeCommitOf), isolationLevel);

            Console.WriteLine("Setting event...");
            evt.Set();

            await Task.WhenAll(t1, t2);

            Console.WriteLine();
            Console.WriteLine("Result:");
            await PostgresDb.Execute(u => u.PrintBookings("Final Read: "));
        }

        public static Func<Actor, Task> BookingRoutineFor(
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

                    Print("Check avaiable bookings");

                    var o = await actor.GetExistingBookingOwnerIfAny(roomId, starTime, endTime);

                    if (o != null)
                    {
                        Print($"Booking for #{roomId} room is owner by {o}. Cannot book.");
                        return;
                    }

                    Print("Booking is avaiable");

                    await actor.Sleep(TimeSpan.FromSeconds(2));

                    await actor.BookRoom(roomId, starTime, endTime, owner, Print);

                    Print("Room is booked");

                    await onBeforeCommitOf(owner);

                    await Task.Delay(_2sec);

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
    }
}
