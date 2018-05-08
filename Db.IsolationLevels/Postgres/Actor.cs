using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace Db.IsolationLevels.Postgres
{
    public class Actor
    {
        public IDbConnection Connection { get; }
        public IDbTransaction Transaction { get; }

        public Actor(IDbConnection connection, IDbTransaction transaction)
        {
            Connection = connection;
            Transaction = transaction;
        }

        public Task BookRoom(int roomId, string startTime, string endTime, string owner, Action<string> reportError)
        {
            try
            {
                return Connection.ExecuteAsync(
                    "INSERT INTO Bookings (RoomId, BeginTime, EndTime, Owner) VALUES (@roomId, @begin, @end, @owner)",
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
                reportError($"Failed to insert into Bookings: {ex.Message}");
                throw;
            }
        }

        public Task<string> GetExistingBookingOwnerIfAny(int roomId, string startTime, string endTime)
        {
            return Connection.QueryFirstOrDefaultAsync<string>(
                "SELECT Owner FROM Bookings WHERE RoomId = @RoomId AND BeginTime BETWEEN @begin AND @end OR EndTime BETWEEN @begin AND @end",
                new
                {
                    begin = TimeSpan.Parse(startTime),
                    end = TimeSpan.Parse(endTime),
                },
                Transaction);
        }

        public Task Sleep(TimeSpan duration)
        {
            return Task.Delay(duration);
        }

        public async Task PrintBookings(string prefix = null, TextWriter writer = null)
        {
            var bookings = await Connection.QueryAsync("SELECT * FROM Bookings", Transaction);
            writer = writer ?? Console.Out;

            writer.WriteLine(bookings.Any()
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
    }
}