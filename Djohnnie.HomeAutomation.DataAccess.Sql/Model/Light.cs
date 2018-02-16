using System;

namespace Djohnnie.HomeAutomation.DataAccess.Sql.Model
{
    public class Light
    {
        public Guid Id { get; set; }
        public Int32 SysId { get; set; }
        public String HostName { get; set; }
        public String MacAddress { get; set; }
        public String Label { get; set; }
    }
}