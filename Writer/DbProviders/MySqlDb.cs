using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;

namespace Writer.DbProviders
{
    public class MySqlDb : IDatabaseProvider
    {
        public string Name => "MySql 5.7";
        
        private static async Task<MySqlConnection> GetOpenConnection()
        {
            var con = new MySqlConnection("Server=localhost;uid=root;Pwd=123; Database=testdb;Port=3306;sslmode=none;Allow User Variables=True");
            await con.OpenAsync();
            return con;
        }

        public async Task ResetDatabase()
        {
            var c = await GetOpenConnection();
            await c.ExecuteAsync("DROP TABLE IF EXISTS testdb.Bookings");
            await c.ExecuteAsync("CREATE TABLE IF NOT EXISTS testdb.Rooms (Id serial primary key, Name varchar(100));");
            await c.ExecuteAsync("INSERT INTO testdb.Rooms (Name) VALUES ('Picasso'), ('Van-Gogh')");
            await c.ExecuteAsync(@"
                        CREATE TABLE IF NOT EXISTS testdb.Bookings (
                                Id serial primary key, 
                                RoomId integer REFERENCES testdb.Rooms(Id), 
                                BeginTime time NOT NULL,
                                EndTime time NOT NULL,
                                Owner varchar(100));");
            await c.ExecuteAsync("CREATE INDEX ix_Booking_room ON Bookings(RoomId)");
        }

        public async Task Transaction(Func<Actor, Task> transactionContent, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, TimeSpan? delay = null, string actorName = null)
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
                        var actor = new MySqlActor(actorName ?? "MySqlActor", con, tran);
                        await transactionContent(actor);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }
}