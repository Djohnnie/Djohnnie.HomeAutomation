using System;
using System.Threading.Tasks;
using Djohnnie.HomeAutomation.DataAccess.Nest.Model;
using RestSharp;

namespace Djohnnie.HomeAutomation.DataAccess.Nest
{
    public class NestRepository : INestRepository
    {
        public async Task<Thermostat> GetCurrentHeating()
        {
            String nestToken = Environment.GetEnvironmentVariable("NEST_TOKEN");
            String nestThermostat = Environment.GetEnvironmentVariable("NEST_THERMOSTAT");
            RestClient client = new RestClient($"https://developer-api.nest.com/devices/thermostats/{nestThermostat}");
            RestRequest request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", $"Bearer {nestToken}");
            IRestResponse<Thermostat> response = await client.ExecuteTaskAsync<Thermostat>(request);
            return response.Data;
        }
    }
}