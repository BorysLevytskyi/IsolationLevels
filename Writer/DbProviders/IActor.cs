using System.Threading.Tasks;
using Writer.Entities;

namespace Writer.DbProviders
{
    public interface IActor
    {
        void Report(string message);
        
        Task<Booking> BookRoom(int roomId = 1, string startTime = "12:00", string endTime = "13:00", string owner = "DefaultOwner");

        Task PrintBookings(string prefix = null);

        Task CommitTransaction();
    }
}