using Djohnnie.HomeAutomation.DataAccess.Lifx;
using Djohnnie.HomeAutomation.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Djohnnie.HomeAutomation.DataAccess.Nest;
using Djohnnie.HomeAutomation.DataAccess.Smappee;

namespace Djohnnie.HomeAutomation.Web.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILifxRepository _lightingRepository;
        private readonly ISmappeeRepository _smappeeRepository;
        private readonly INestRepository _nestRepository;

        public HomeController(ILifxRepository lightingRepository, ISmappeeRepository smappeeRepository, INestRepository nestRepository)
        {
            _lightingRepository = lightingRepository;
            _smappeeRepository = smappeeRepository;
            _nestRepository = nestRepository;
        }

        public IActionResult Index()
        {
            AddPageHeader("Dashboard", "");
            return View();
        }

        public async Task<IActionResult> GetLivePowerUsage()
        {
            try
            {
                LivePowerUsageViewModel vm = new LivePowerUsageViewModel();
                var livePowerUsage = await _smappeeRepository.GetLivePowerUsage("192.168.10.191");
                vm.CurrentPower = livePowerUsage.ActivePower;
                var powerConsumption = await _smappeeRepository.GetPowerConsumptionToday();
                vm.Consumption = (powerConsumption.Consumptions.SingleOrDefault()?.Consumption ?? 0) / 1000M;
                return PartialView("_LivePowerUsage", vm);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task<IActionResult> GetLiveHeatingOverview()
        {
            try
            {
                LiveHeatingOverviewViewModel vm = new LiveHeatingOverviewViewModel();
                var heating = await _nestRepository.GetCurrentHeating();
                vm.CurrentTemperature = heating.AmbientTemperatureC;
                vm.CurrentHumidity = heating.Humidity;
                return PartialView("_LiveHeatingOverview", vm);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task<IActionResult> GetLiveLightingOverview()
        {
            try
            {
                LiveLightingOverviewViewModel vm = new LiveLightingOverviewViewModel();
                vm.Lights = await _lightingRepository.GetLights();
                return PartialView("_LiveLightingOverview", vm);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public Task<IActionResult> GetLivePlugsOverview()
        {
            LivePlugsOverviewViewModel vm = new LivePlugsOverviewViewModel();
            return Task.FromResult((IActionResult)PartialView("_LivePlugsOverview", vm));
        }
    }

    public class Blablabla
    {
        public String Report { get; set; }
    }
}