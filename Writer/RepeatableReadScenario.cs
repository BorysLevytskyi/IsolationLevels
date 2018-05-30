using System;
using System.Data;
using System.Threading.Tasks;
using Writer.DbProviders;

namespace Writer
{
    public class RepeatableReadScenario
    {
        public static async Task Definition(IDatabaseProvider provider, IsolationLevel isolationLevel)
        {
            await provider.ResetDatabase();
            int bookingId = 0;
            
            await provider.Transaction(async a =>
            {
                var booking = await a.BookRoom();
                Console.WriteLine($"Booking #{booking.Id}");
                a.CommitTransaction();
                bookingId = booking.Id;
            }, isolationLevel);
            
            await provider.Transaction(async a =>
            {
                await a.PrintBookings("Before Delete");
                
                await a.PrintBookings("After Delete");
                
                a.CommitTransaction();
                
            }, IsolationLevel.RepeatableRead);

            await provider.Transaction(async a =>
            {
                await a.DeleteBooking(bookingId);
                a.CommitTransaction();
                a.Print("Booking Deleted");
                await provider.Transaction(p => p.PrintBookings("3rd person"));
            });
        }
    }
}