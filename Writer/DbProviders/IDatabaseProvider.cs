using System;
using System.Data;
using System.Threading.Tasks;

namespace Writer.DbProviders
{
    public interface IDatabaseProvider
    {
        string Name { get; }
        
        Task ResetDatabase();
        
        Task Transaction(Func<Actor, Task> transactionContent, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, TimeSpan? delay = null, string actorName = null);
    }
}