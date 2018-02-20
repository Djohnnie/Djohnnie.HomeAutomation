using System;

namespace Djohnnie.HomeAutomation.DataAccess.Nest.Model
{
    public class Thermostat
    {
        public String Name { get; set; }
        public Decimal Humidity { get; set; }
        public Decimal AmbientTemperatureC { get; set; }
        public Decimal TargetTemperatureC { get; set; }
        public String TimeToTarget { get; set; }
    }
}