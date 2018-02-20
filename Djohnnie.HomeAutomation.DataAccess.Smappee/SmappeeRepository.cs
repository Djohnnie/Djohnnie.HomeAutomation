using System;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Djohnnie.HomeAutomation.DataAccess.Smappee.Model;
using Djohnnie.HomeAutomation.Utils;
using RestSharp;

namespace Djohnnie.HomeAutomation.DataAccess.Smappee
{
    public class SmappeeRepository : ISmappeeRepository
    {
        private String _smappeeToken;
        private String _serviceLocation;

        public async Task<LivePowerUsage> GetLivePowerUsage(String hostName)
        {
            RestClient client = new RestClient($"http://{hostName}/gateway/apipublic/reportInstantaneousValues");
            IRestResponse<String> response = await client.ExecuteTaskAsync<String>(
                new RestRequest(Method.GET) { RequestFormat = DataFormat.Json, OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; } });
            LivePowerUsage result = new LivePowerUsage();
            if (response.IsSuccessful && response.Data != null)
            {
                foreach (Match m in Regex.Matches(response.Data, @"voltage=(\d*\.?\d+) Vrms"))
                {
                    result.Voltage = (int)Math.Round(Convert.ToDecimal(m.Groups[1].Value, CultureInfo.InvariantCulture));
                }
                foreach (Match m in Regex.Matches(response.Data, @"current=(\d*\.?\d+) A"))
                {
                    result.Current = (int)Math.Round(Convert.ToDecimal(m.Groups[1].Value, CultureInfo.InvariantCulture));
                }
                foreach (Match m in Regex.Matches(response.Data, @"activePower=(\d*\.?\d+) W"))
                {
                    result.ActivePower = (int)Math.Round(Convert.ToDecimal(m.Groups[1].Value, CultureInfo.InvariantCulture));
                }
                foreach (Match m in Regex.Matches(response.Data, @"reactivePower=(\d*\.?\d+) var"))
                {
                    result.ReactivePower = (int)Math.Round(Convert.ToDecimal(m.Groups[1].Value, CultureInfo.InvariantCulture));
                }
                foreach (Match m in Regex.Matches(response.Data, @"apparentPower=(\d*\.?\d+) VA"))
                {
                    result.ApparentPower = (int)Math.Round(Convert.ToDecimal(m.Groups[1].Value, CultureInfo.InvariantCulture));
                }
            }
            return result;
        }

        public async Task<PowerConsumptionList> GetPowerConsumptionToday()
        {
            if (String.IsNullOrEmpty(_smappeeToken))
            {
                _smappeeToken = await GetSmappeeToken();
            }

            if (String.IsNullOrEmpty(_serviceLocation))
            {
                _serviceLocation = await GetServiceLocation();
            }
            
            Int64 from = DateTime.Today.ConvertDateTimeToEpoch();
            Int64 to = DateTime.Today.AddDays(1).AddMilliseconds(-1).ConvertDateTimeToEpoch();
            Console.WriteLine($"From {from} to {to}");
            RestClient client = new RestClient($"https://app1pub.smappee.net/dev/v1/servicelocation/{_serviceLocation}/consumption?aggregation=3&from={from}&to={to}");
            RestRequest request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", $"Bearer {_smappeeToken}");
            IRestResponse<PowerConsumptionList> response = await client.ExecuteTaskAsync<PowerConsumptionList>(request);
            Console.WriteLine(response.Content);
            return response.Data;
        }

        private async Task<String> GetSmappeeToken()
        {
            String clientId = Environment.GetEnvironmentVariable("SMAPPEE_CLIENT_ID");
            String clientSecret = Environment.GetEnvironmentVariable("SMAPPEE_CLIENT_SECRET");
            String username = Environment.GetEnvironmentVariable("SMAPPEE_USERNAME");
            String password = Environment.GetEnvironmentVariable("SMAPPEE_PASSWORD");
            RestClient client = new RestClient("https://app1pub.smappee.net/dev/v1/oauth2/token");
            RestRequest request = new RestRequest(Method.POST);
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", $"grant_type=password&client_id={clientId}&client_secret={clientSecret}&username={username}&password={password}", ParameterType.RequestBody);
            IRestResponse<SmappeeToken> response = await client.ExecuteTaskAsync<SmappeeToken>(request);
            Console.WriteLine(response.Content);
            return response.Data.AccessToken;
        }

        private async Task<String> GetServiceLocation()
        {
            RestClient client = new RestClient("https://app1pub.smappee.net/dev/v1/servicelocation");
            RestRequest request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", $"Bearer {_smappeeToken}");
            IRestResponse<ServiceLocationList> response = await client.ExecuteTaskAsync<ServiceLocationList>(request);
            Console.WriteLine(response.Content);
            return response.Data.ServiceLocations[0].ServiceLocationId;
        }
    }
}