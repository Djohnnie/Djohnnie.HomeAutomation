using Djohnnie.HomeAutomation.DataAccess.Lifx.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Djohnnie.HomeAutomation.DataAccess.Lifx
{
    public interface ILightingRepository
    {
        Task<List<Light>> GetLights();
    }
}