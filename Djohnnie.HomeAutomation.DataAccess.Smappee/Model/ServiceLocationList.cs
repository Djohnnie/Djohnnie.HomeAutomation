using System;
using System.Collections.Generic;

namespace Djohnnie.HomeAutomation.DataAccess.Smappee.Model
{
    public class ServiceLocationList
    {
        public String AppName { get; set; }
        public List<ServiceLocation> ServiceLocations { get; set; }
    }
}