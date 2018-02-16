using System;

namespace Djohnnie.HomeAutomation.DataAccess.Smappee.Model
{
    public class LivePowerUsage
    {
        public Decimal Voltage { get; set; }
        public Decimal Current { get; set; }
        public Decimal ActivePower { get; set; }
        public Decimal ReactivePower { get; set; }
        public Decimal ApparentPower { get; set; }
    }
}