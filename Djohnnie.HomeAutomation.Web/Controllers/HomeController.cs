using Djohnnie.HomeAutomation.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Djohnnie.HomeAutomation.Web.Controllers
{
    public class HomeController : BaseController
    {
        public IActionResult Index()
        {
            AddPageHeader("Dashboard", "");
            return View();
        }

        public async Task<IActionResult> GetLivePowerUsage()
        {
            LivePowerUsageViewModel vm = new LivePowerUsageViewModel()
            {
                CurrentPower = new Random().Next(100, 5000)
            };

            RestClient client = new RestClient("http://192.168.10.92/gateway/apipublic/reportInstantaneousValues");
            var response = await client.ExecuteTaskAsync<Blablabla>(
                new RestRequest(Method.GET) { RequestFormat = DataFormat.Json, OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; } });
            if (response.IsSuccessful && response.Data != null)
            {
                foreach (Match m in Regex.Matches(response.Data.Report, @"activePower=(\d*\.?\d+) W"))
                {
                    vm.CurrentPower = (int)Math.Round(Convert.ToDecimal(m.Groups[1].Value, CultureInfo.InvariantCulture));
                }
            }

            return PartialView("_LivePowerUsage", vm);
        }
    }

    public class Blablabla
    {
        public String Report { get; set; }
    }
}