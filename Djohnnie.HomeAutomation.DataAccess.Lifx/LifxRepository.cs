using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Djohnnie.HomeAutomation.DataAccess.Lifx.Communication;
using Djohnnie.HomeAutomation.DataAccess.Lifx.Model;

namespace Djohnnie.HomeAutomation.DataAccess.Lifx
{
    public class LifxRepository : ILifxRepository
    {
        private LifxClient _lifxClient;

        public async Task<List<Light>> GetLights()
        {
            if (_lifxClient == null)
            {
                _lifxClient = await LifxClient.CreateAsync();
                _lifxClient.DeviceDiscovered += LifxClient_DeviceDiscovered;
                _lifxClient.StartDeviceDiscovery();
            }

            List<Light> lights = new List<Light>();
            foreach (Device device in _lifxClient.Devices)
            {
                if (device is LightBulb bulb)
                {
                    Ping ping = new Ping();
                    PingReply reply = await ping.SendPingAsync(bulb.HostName, 100);
                    if (reply.Status == IPStatus.Success)
                    {
                        LightStateResponse response = await _lifxClient.GetLightStateAsync(bulb);
                        lights.Add(new Light { Description = response.Label, IsOn = response.IsOn });
                    }
                }
            }

            return lights;
        }

        private void LifxClient_DeviceDiscovered(object sender, LifxClient.DeviceDiscoveryEventArgs e)
        {
            Debug.WriteLine(e.Device.HostName);
        }
    }
}