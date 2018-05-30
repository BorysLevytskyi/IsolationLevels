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

            await provider.Transaction(async a =>
            {
                var booking = await a.BookRoom();
                Console.WriteLine($"Booking #{booking.Id}");
                a.CommitTransaction();
            }, isolationLevel);
        }
    }
}