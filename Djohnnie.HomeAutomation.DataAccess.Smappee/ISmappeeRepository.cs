using System;
using System.Threading.Tasks;
using Djohnnie.HomeAutomation.DataAccess.Smappee.Model;

namespace Djohnnie.HomeAutomation.DataAccess.Smappee
{
    public interface ISmappeeRepository
    {
        Task<LivePowerUsage> GetLivePowerUsage(String hostName);

        Task<PowerConsumptionList> GetPowerConsumptionToday();
    }
}