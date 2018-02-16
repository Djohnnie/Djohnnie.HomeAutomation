using System;

namespace Djohnnie.HomeAutomation.DataAccess.Smappee.Model
{
    public class PowerConsumption
    {
        public Int64 TimeStamp { get; set; }
        public Decimal Consumption { get; set; }
        public Decimal AlwaysOn { get; set; }
    }
}