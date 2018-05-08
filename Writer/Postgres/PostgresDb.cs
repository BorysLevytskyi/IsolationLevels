using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Npgsql;

namespace Writer.Postgres
{
    public class PostgresDb : IDatabaseProvider
    {
        public string Name => "Postgres";
            
        private async Task<NpgsqlConnection> GetOpenConnection()
        {
            var con = new NpgsqlConnection("Host=localhost;User ID=admin;Password=123; Database=testdb");
            await con.OpenAsync();
            return con;
        }

        public async Task ResetDatabase()
        {
            var c = await GetOpenConnection();
            await c.ExecuteAsync("DROP TABLE IF EXISTS Bookings");
            await c.ExecuteAsync("CREATE TABLE IF NOT EXISTS Rooms (Id serial primary key, Name varchar(100));");
            await c.ExecuteAsync("INSERT INTO Rooms (Name) VALUES ('Picasso'), ('Van-Gogh')");
            await c.ExecuteAsync(@"
                        CREATE TABLE IF NOT EXISTS Bookings (
                                Id serial primary key, 
                                RoomId integer REFERENCES public.Rooms(Id), 
                                BeginTime time NOT NULL,
                                EndTime time NOT NULL,
                                Owner varchar(100));");
            await c.ExecuteAsync("CREATE INDEX ix_Booking_room ON Bookings(RoomId)");

        }

        public async Task Transaction(Func<Actor, Task> transactionContent, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, TimeSpan? delay = null)
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