using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Writer.Postgres;
using Npgsql;
using Console = System.Console;
using static Writer.Times;
// ReSharper disable InconsistentNaming

namespace Writer
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Started");

            try
            {
                var isolationLevel = IsolationLevel.ReadCommitted;
                var scenario = GetScenario();
                Console.WriteLine("------- MySql -----------------------------------");
                RunMysql(scenario, isolationLevel).GetAwaiter().GetResult();
                Console.WriteLine();
                Console.WriteLine("------ Postgres -----------------------------------");
                RunPostgres(scenario, isolationLevel).GetAwaiter().GetResult();

                Console.WriteLine("Done");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static async Task RunPostgres(ScenarioAction run, IsolationLevel isolationLevel)
        {
            var p = new PostgresDb();
            await run(p.ResetDatabase, p.ExecuteTransaction, isolationLevel);
        }

        private static async Task RunMysql(ScenarioAction run, IsolationLevel isolationLevel)
        {
            var mysql = new MySqlDb();
            await run(mysql.ResetDatabase, mysql.ExecuteTransaction, isolationLevel);
        }

        private static ScenarioAction GetScenario()
        {
            return DoubleBookingScenario.Definition;
        }
    }
}
