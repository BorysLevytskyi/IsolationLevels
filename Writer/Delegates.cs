using System.Data;
using System.Threading.Tasks;
using Writer.DbProviders;

namespace Writer
{
    public delegate Task ScenarioAction(
        IDatabaseProvider db,
        IsolationLevel isolationLevel);
}