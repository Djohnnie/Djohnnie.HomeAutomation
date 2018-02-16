using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Djohnnie.HomeAutomation.DataAccess.Smappee.Model;
using Djohnnie.HomeAutomation.Utils;
using RestSharp;

namespace Djohnnie.HomeAutomation.DataAccess.Smappee
{
    public class SmappeeRepository : ISmappeeRepository
    {
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

        public async Task<PowerConsumptionList> GetPowerConsumptionToday(String serviceLocation, String token)
        {
            Int64 from = DateTime.Today.ConvertDateTimeToEpoch();
            Int64 to = DateTime.Today.AddDays(1).AddMilliseconds(-1).ConvertDateTimeToEpoch();
            RestClient client = new RestClient($"https://app1pub.smappee.net/dev/v1/servicelocation/{serviceLocation}/consumption?aggregation=3&from={from}&to={to}");
            RestRequest request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", $"Bearer {token}");
            IRestResponse<PowerConsumptionList> response = await client.ExecuteTaskAsync<PowerConsumptionList>(request);
            return response.Data;
        }
    }
}