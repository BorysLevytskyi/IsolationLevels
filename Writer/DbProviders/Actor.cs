using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Writer.Entities;

namespace Writer.DbProviders
{
    public abstract class Actor
    {
        public IDbConnection Connection { get; }
        
        public IDbTransaction Transaction { get; }
        
        public string Name { get; }

        protected Actor(string name, IDbConnection connection, IDbTransaction transaction)
        {
            Connection = connection;
            Transaction = transaction;
            Name = name;
        }

        private static readonly object Lock = new object();

        public void Print(string message)
        {
            lock (Lock)
            {
                Console.WriteLine(message);
            }
        }

        public Task<Booking> BookRoom(int roomId = 1, string startTime = "12:00", string endTime = "13:00", string owner = "DefaultOwner")
        {
            try
            {
                var bookingSql = GetNewBookingSql();

                return Connection.QuerySingleAsync<Booking>(
                    bookingSql,
                    new
                    {
                        roomId,
                        begin = TimeSpan.Parse(startTime),
                        end = TimeSpan.Parse(endTime),
                        owner
                    },
                    Transaction);
            }
            catch (Exception ex)
            {
                Print($"Error: Failed to insert into Bookings: {ex.Message}");
                throw;
            }
        }

        public Task<string> GetExistingBookingOwnerIfAny(int roomId, string startTime, string endTime)
        {
            return Connection.QueryFirstOrDefaultAsync<string>(
                "SELECT Owner FROM Bookings WHERE RoomId = @RoomId AND BeginTime BETWEEN @begin AND @end OR EndTime BETWEEN @begin AND @end",
                new
                {
                    roomId,
                    begin = TimeSpan.Parse(startTime),
                    end = TimeSpan.Parse(endTime),
                },
                Transaction);
        }

        public Task Sleep(TimeSpan duration)
        {
            return Task.Delay(duration);
        }

        public async Task PrintBookings(string prefix = null)
        {
            var bookings = (await Connection.QueryAsync("SELECT * FROM Bookings", Transaction)).ToList();
            
            Print(bookings.Any()
                ? $"{prefix}{string.Join(Environment.NewLine, bookings)}"
                : $"{prefix}No bookings at this time. IsolationLevel={Transaction.IsolationLevel}");
        }

        public void CommitTransaction(Action<string> report = null) 
        {
            try
            {
                Transaction.Commit();
            }
            catch (Exception e)
            {
                report?.Invoke($"Failed to commit transaction: {e.Message}");
                throw e;
            }
        }

        protected abstract string GetNewBookingSql();
    }
}