using System;
using System.Data;
using System.Threading.Tasks;
using Db.IsolationLevels.Postgres;

namespace Db.IsolationLevels
{
    public delegate Task ExecuteDelegate(Func<Actor, Task> transactionContent, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, TimeSpan? delay = null);
}