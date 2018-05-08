using System;
using System.Data;
using System.Threading.Tasks;

namespace Writer.Postgres
{
    public interface IDatabaseProvider
    {
        Task ResetDatabase();
        Task ExecuteTransaction(Func<Actor, Task> transactionContent, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, TimeSpan? delay = null);
    }
}