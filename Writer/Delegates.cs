using System.Data;
using System.Threading.Tasks;
using Writer.Postgres;

namespace Writer
{
    public delegate Task ScenarioAction(
        IDatabaseProvider db,
        IsolationLevel isolationLevel);
}