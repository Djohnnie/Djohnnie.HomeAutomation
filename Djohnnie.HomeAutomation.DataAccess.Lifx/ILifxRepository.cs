using Djohnnie.HomeAutomation.DataAccess.Lifx.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Djohnnie.HomeAutomation.DataAccess.Lifx
{
    public interface ILifxRepository
    {
        Task<List<Light>> GetLights();
    }
}