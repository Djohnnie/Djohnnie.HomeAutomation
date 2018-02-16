using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Djohnnie.HomeAutomation.DataAccess.Sql.Model;
using Microsoft.EntityFrameworkCore;

namespace Djohnnie.HomeAutomation.DataAccess.Sql
{
    public class LightRepository : ILightRepository
    {
        private readonly HomeAutomationDbContext _dbContext;

        public LightRepository(HomeAutomationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<List<Light>> GetLights()
        {
            return _dbContext.Lights.ToListAsync();
        }

        public async Task<Light> CreateLight(Light light)
        {
            _dbContext.Lights.Add(light);
            await _dbContext.SaveChangesAsync();
            return light;
        }

        public Task<Light> UpdateLight(Light light)
        {
            return Task.FromResult(light);
        }
    }
}