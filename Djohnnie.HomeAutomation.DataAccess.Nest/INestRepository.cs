using System.Threading.Tasks;
using Djohnnie.HomeAutomation.DataAccess.Nest.Model;

namespace Djohnnie.HomeAutomation.DataAccess.Nest
{
    public interface INestRepository
    {
        Task<Thermostat> GetCurrentHeating();
    }
}