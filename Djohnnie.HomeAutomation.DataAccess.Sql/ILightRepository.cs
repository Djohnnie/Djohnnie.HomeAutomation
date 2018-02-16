using Djohnnie.HomeAutomation.DataAccess.Sql.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Djohnnie.HomeAutomation.DataAccess.Sql
{
    public interface ILightRepository
    {
        Task<List<Light>> GetLights();

        Task<Light> CreateLight(Light light);

        Task<Light> UpdateLight(Light light);
    }
}