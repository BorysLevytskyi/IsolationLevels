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
                    #SELECT last_insert_id() as Id
                    SELECT Id, RoomId, cast(BeginTime as char) as BeginTime, cast(EndTime as char) as EndTime, Owner FROM Bookings WHERE Id = last_insert_id()";
        }
    }
}