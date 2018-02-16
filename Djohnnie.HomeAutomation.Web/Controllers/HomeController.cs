using Djohnnie.HomeAutomation.DataAccess.Lifx;
using Djohnnie.HomeAutomation.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Djohnnie.HomeAutomation.DataAccess.Smappee;

namespace Djohnnie.HomeAutomation.Web.Controllers
{
    public class HomeController : BaseController
    {
        private ILifxRepository _lightingRepository;
        private ISmappeeRepository _smappeeRepository;

        public HomeController(ILifxRepository lightingRepository, ISmappeeRepository smappeeRepository)
        {
            _lightingRepository = lightingRepository;
            _smappeeRepository = smappeeRepository;
        }

        public IActionResult Index()
        {
            AddPageHeader("Dashboard", "");
            return View();
        }

        public async Task<IActionResult> GetLivePowerUsage()
        {
            LivePowerUsageViewModel vm = new LivePowerUsageViewModel();
            var livePowerUsage = await _smappeeRepository.GetLivePowerUsage("192.168.10.191");
            vm.CurrentPower = livePowerUsage.ActivePower;
            var powerConsumption = await _smappeeRepository.GetPowerConsumptionToday("28607", "3833a9f3-b773-3b35-a0a2-9ca6c9dc3611");
            vm.Consumption = powerConsumption.Consumptions.Single().Consumption / 1000M;
            return PartialView("_LivePowerUsage", vm);
        }

        public Task<IActionResult> GetLiveHeatingOverview()
        {
            LiveHeatingOverviewViewModel vm = new LiveHeatingOverviewViewModel();
            return Task.FromResult((IActionResult)PartialView("_LiveHeatingOverview", vm));
        }

        public async Task<IActionResult> GetLiveLightingOverview()
        {
            try
            {
                LiveLightingOverviewViewModel vm = new LiveLightingOverviewViewModel();
                vm.Lights = await _lightingRepository.GetLights();
                return PartialView("_LiveLightingOverview", vm);
            }
            catch
            {
                return PartialView("_LiveLightingOverview", new LiveLightingOverviewViewModel());
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