using System;

namespace Djohnnie.HomeAutomation.Web.ViewModels
{
    public class LiveHeatingOverviewViewModel
    {
        public Decimal CurrentTemperature { get; set; }
        public Decimal CurrentHumidity { get; set; }
    }
}