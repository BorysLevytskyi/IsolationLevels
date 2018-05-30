using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Google.Protobuf.WellKnownTypes;
using Npgsql;
using Writer.DbProviders;
using Console = System.Console;
using static Writer.Times;
// ReSharper disable InconsistentNaming

namespace Writer
{
    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var isolationLevel = IsolationLevel.Serializable;
                var scenario = GetScenario();
                foreach (var db in GetDatabaseProviders(args.First()))
                {
                    Console.WriteLine($"------- {db.Name} -----------------------------------");
                    scenario(db, isolationLevel).GetAwaiter().GetResult();
                }
                
                Console.WriteLine("Done");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static ScenarioAction GetScenario()
        {
            //return DoubleBookingScenario.Definition;
            return RepeatableReadScenario.Definition;
        }

        private static IEnumerable<IDatabaseProvider> GetDatabaseProviders(string filter)
        {
            var pg = new PostgresDb();
            var msql = new MySqlDb();

            if (!string.IsNullOrEmpty(filter))
            {
                switch (filter)
                {
                    case "m": 
                    case "my":
                        yield return msql;
                        yield break;
                    case "p": 
                    case "pg":
                        yield return pg;
                        yield break;   
                }
            }

            yield return msql;
            yield return pg;
        }
    }
}
