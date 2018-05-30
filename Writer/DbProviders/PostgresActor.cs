using System.Data;

namespace Writer.DbProviders
{
    public class PostgresActor : Actor
    {
        public PostgresActor(string name, IDbConnection connection, IDbTransaction transaction) : base(name, connection, transaction)
        {
        }

        protected override string GetNewBookingSql()
        {
            return @"INSERT INTO Bookings (RoomId, BeginTime, EndTime, Owner) 
                    VALUES (@roomId, @begin, @end, @owner)
                    RETURNING Id, RoomId, BeginTime, EndTime, Owner";
        }
    }
}