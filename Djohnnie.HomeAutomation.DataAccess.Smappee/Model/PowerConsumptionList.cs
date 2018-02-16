using System;
using System.Collections.Generic;

namespace Djohnnie.HomeAutomation.DataAccess.Smappee.Model
{
    public class PowerConsumptionList
    {
        public String ServiceLocationId { get; set; }
        public List<PowerConsumption> Consumptions { get; set; }
    }
}