using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Npgsql;

namespace Writer.Postgres
{
    public class PostgresDb : IDatabaseProvider
    {
        private async Task<NpgsqlConnection> GetOpenConnection()
        {
            var con = new NpgsqlConnection("Host=localhost;User ID=admin;Password=123; Database=testdb");
            await con.OpenAsync();
            return con;
        }

        public async Task ResetDatabase()
        {
            var c = await GetOpenConnection();
            await c.ExecuteAsync("DROP TABLE IF EXISTS public.Bookings");
            await c.ExecuteAsync("CREATE TABLE IF NOT EXISTS public.Rooms (Id serial primary key, Name varchar(100));");
            await c.ExecuteAsync("INSERT INTO public.Rooms (Name) VALUES ('Picasso'), ('Van-Gogh')");
            await c.ExecuteAsync(@"
                        CREATE TABLE IF NOT EXISTS public.Bookings (
                                Id serial primary key, 
                                RoomId integer REFERENCES public.Rooms(Id), 
                                BeginTime time NOT NULL,
                                EndTime time NOT NULL,
                                Owner varchar(100));");

        }

        public async Task ExecuteTransaction(Func<Actor, Task> transactionContent, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, TimeSpan? delay = null)
        {
            if (delay != null)
            {
                await Task.Delay(delay.Value);
            }

            using (var con = await GetOpenConnection())
            { 
                try
                {
                    using (var tran = con.BeginTransaction(isolationLevel))
                    {
                        var actor = new Actor(con, tran);
                        await transactionContent(actor);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Transaction failed: {e.Message}");
                }
            }
        }
    }
}