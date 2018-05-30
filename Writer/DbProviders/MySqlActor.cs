using System.Data;

namespace Writer.DbProviders
{
    public class MySqlActor : Actor
    {
        public MySqlActor(string name, IDbConnection connection, IDbTransaction transaction) : base(name, connection, transaction)
        {
        }

        protected override string GetNewBookingSql()
        {
            return @"INSERT INTO Bookings (RoomId, BeginTime, EndTime, Owner) 
                    VALUES (@roomId, @begin, @end, @owner);
                    SELECT Id, RoomId, BeginTime, EndTime, Owner FROM Bookings WHERE Id = last_insert_id()";
        }
    }
}