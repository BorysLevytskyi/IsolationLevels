using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Db.IsolationLevels;
using Db.IsolationLevels.Postgres;

namespace Reader
{
    class Program
    {
        static void Main(string[] args)
        {

            var cts = new CancellationTokenSource();
            var level = IsolationLevel.RepeatableRead;
            var pg = RunReads(cts, PostgresDb.Execute, "Postgres", level);
            var mysql = RunReads(cts, MySqlDb.Execute, "MySql", level);

            Console.WriteLine($"{level} Press any key to stop");

            var booking = RunBooking();

            Console.ReadKey();
            Console.WriteLine();
            cts.Cancel();

            Task.WhenAll(pg, mysql).GetAwaiter().GetResult();
            booking.WaitForExit();
        }

        private static Process RunBooking()
        {
            var inf = new ProcessStartInfo("Db.IsolationLevels.exe")
            {
                //RedirectStandardOutput = true,
                //CreateNoWindow = true,
                //UseShellExecute = false
            };

            var p = Process.Start(inf);
            return p;
        }

        private static async Task RunReads(CancellationTokenSource cts, ExecuteDelegate execute, string db, IsolationLevel isolationLevel)
        {
            TimeSpan maxTime = TimeSpan.Zero;
            

            await Task.Yield();
            var sw = new Stopwatch();

            while (!cts.IsCancellationRequested)
            {
                Thread.Sleep(100);

                sw.Restart();
                await execute(r => r.PrintBookings(writer: TextWriter.Null));
                sw.Stop();

                var elapsed = sw.Elapsed;
                maxTime = sw.Elapsed;

                if (maxTime < elapsed)
                {
                    maxTime = elapsed;
                }
            }

            Console.WriteLine($"{db} max time between reads: {maxTime.TotalMilliseconds}ms");
        }
    }
}
