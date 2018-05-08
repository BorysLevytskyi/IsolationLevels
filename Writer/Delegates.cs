using System;
using System.Data;
using System.Threading.Tasks;
using Writer.Postgres;

namespace Writer
{
    public delegate Task ScenarioAction(
        SetupAction start, 
        ExecuteAction execute,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

    public delegate Task SetupAction();

    public delegate Task ExecuteAction(
        Func<Actor, Task> transactionContent, 
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, 
        TimeSpan? delay = null);
}