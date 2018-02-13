using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Djohnnie.HomeAutomation.DataAccess.Lifx.Communication
{
    /// <summary>
	/// LIFX Client for communicating with bulbs
	/// </summary>
	public class LifxClient : IDisposable
    {

        private const int Port = 56700;
        private UdpClient _socket;
        private bool _isRunning;

        private LifxClient()
        {
        }

        /// <summary>
        /// Creates a new LIFX client.
        /// </summary>
        /// <returns>client</returns>
        public static Task<LifxClient> CreateAsync()
        {
            LifxClient client = new LifxClient();
            client.Initialize();
            return Task.FromResult(client);
        }

        private void Initialize()
        {
            IPEndPoint end = new IPEndPoint(IPAddress.Any, Port);
            _socket = new UdpClient(end);
            _socket.Client.Blocking = false;
            _socket.DontFragment = true;
            _socket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _isRunning = true;
            StartReceiveLoop();
        }

        private void StartReceiveLoop()
        {
            Task.Run(async () =>
            {
                while (_isRunning)
                    try
                    {
                        var result = await _socket.ReceiveAsync();
                        if (result.Buffer.Length > 0)
                        {
                            HandleIncomingMessages(result.Buffer, result.RemoteEndPoint);
                        }
                    }
                    catch { }
            });
        }

        private void HandleIncomingMessages(byte[] data, System.Net.IPEndPoint endpoint)
        {
            var remote = endpoint;
            var msg = ParseMessage(data);
            if (msg.Type == MessageType.DeviceStateService)
            {
                ProcessDeviceDiscoveryMessage(remote.Address, remote.Port, msg);
            }
            else
            {
                if (taskCompletions.ContainsKey(msg.Source))
                {
                    var tcs = taskCompletions[msg.Source];
                    tcs(msg);
                }
                else
                {
                    //TODO
                }
            }
            System.Diagnostics.Debug.WriteLine("Received from {0}:{1}", remote.ToString(),
                string.Join(",", (from a in data select a.ToString("X2")).ToArray()));

        }

        /// <summary>
        /// Disposes the client
        /// </summary>
        public void Dispose()
        {
            _isRunning = false;
            _socket.Dispose();
        }

        private Task<T> BroadcastMessageAsync<T>(string hostName, FrameHeader header, MessageType type, params object[] args)
                        where T : LifxResponse

        {
            List<byte> payload = new List<byte>();
            if (args != null)
            {
                foreach (var arg in args)
                {
                    if (arg is UInt16)
                        payload.AddRange(BitConverter.GetBytes((UInt16)arg));
                    else if (arg is UInt32)
                        payload.AddRange(BitConverter.GetBytes((UInt32)arg));
                    else if (arg is byte)
                        payload.Add((byte)arg);
                    else if (arg is byte[])
                        payload.AddRange((byte[])arg);
                    else if (arg is string)
                        payload.AddRange(Encoding.UTF8.GetBytes(((string)arg).PadRight(32).Take(32).ToArray())); //All strings are 32 bytes
                    else
                        throw new NotSupportedException(args.GetType().FullName);
                }
            }
            return BroadcastMessagePayloadAsync<T>(hostName, header, type, payload.ToArray());
        }
        private async Task<T> BroadcastMessagePayloadAsync<T>(string hostName, FrameHeader header, MessageType type, byte[] payload)
            where T : LifxResponse
        {
            if (hostName == null)
            {
                hostName = "192.168.10.255";
            }
            TaskCompletionSource<T> tcs = null;
            if (//header.AcknowledgeRequired && 
                header.Identifier > 0 &&
                typeof(T) != typeof(UnknownResponse))
            {
                tcs = new TaskCompletionSource<T>();
                Action<LifxResponse> action = (r) =>
                {
                    if (!tcs.Task.IsCompleted)
                    {
                        if (r.GetType() == typeof(T))
                            tcs.SetResult((T)r);
                        else
                        {

                        }
                    }
                };
                taskCompletions[header.Identifier] = action;
            }

            using (MemoryStream stream = new MemoryStream())
            {
                await WritePacketToStreamAsync(stream, header, (UInt16)type, payload).ConfigureAwait(false);
                var msg = stream.ToArray();
                await _socket.SendAsync(msg, msg.Length, hostName, Port);
            }
            //{
            //	await WritePacketToStreamAsync(stream, header, (UInt16)type, payload).ConfigureAwait(false);
            //}
            T result = default(T);
            if (tcs != null)
            {
                var _ = Task.Delay(1000).ContinueWith((t) =>
                {
                    if (!t.IsCompleted)
                        tcs.TrySetException(new TimeoutException());
                });
                try
                {
                    result = await tcs.Task.ConfigureAwait(false);
                }
                finally
                {
                    taskCompletions.Remove(header.Identifier);
                }
            }
            return result;
        }

        private LifxResponse ParseMessage(byte[] packet)
        {
            using (MemoryStream ms = new MemoryStream(packet))
            {
                var header = new FrameHeader();
                BinaryReader br = new BinaryReader(ms);
                //frame
                var size = br.ReadUInt16();
                if (packet.Length != size || size < 36)
                    throw new Exception("Invalid packet");
                var a = br.ReadUInt16(); //origin:2, reserved:1, addressable:1, protocol:12
                var source = br.ReadUInt32();
                //frame address
                byte[] target = br.ReadBytes(8);
                header.TargetMacAddress = target;
                ms.Seek(6, SeekOrigin.Current); //skip reserved
                var b = br.ReadByte(); //reserved:6, ack_required:1, res_required:1, 
                header.Sequence = br.ReadByte();
                //protocol header
                var nanoseconds = br.ReadUInt64();
                header.AtTime = Utilities.Epoch.AddMilliseconds(nanoseconds * 0.000001);
                var type = (MessageType)br.ReadUInt16();
                ms.Seek(2, SeekOrigin.Current); //skip reserved
                byte[] payload = null;
                if (size > 36)
                    payload = br.ReadBytes(size - 36);
                return LifxResponse.Create(header, type, source, payload);
            }
        }

        private async Task WritePacketToStreamAsync(Stream outStream, FrameHeader header, UInt16 type, byte[] payload)
        {
            using (var dw = new BinaryWriter(outStream) { /*ByteOrder = ByteOrder.LittleEndian*/ })
            {
                //BinaryWriter bw = new BinaryWriter(ms);
                #region Frame
                //size uint16
                dw.Write((UInt16)((payload != null ? payload.Length : 0) + 36)); //length
                                                                                 // origin (2 bits, must be 0), reserved (1 bit, must be 0), addressable (1 bit, must be 1), protocol 12 bits must be 0x400) = 0x1400
                dw.Write((UInt16)0x3400); //protocol
                dw.Write((UInt32)header.Identifier); //source identifier - unique value set by the client, used by responses. If 0, responses are broadcasted instead
                #endregion Frame

                #region Frame address
                //The target device address is 8 bytes long, when using the 6 byte MAC address then left - 
                //justify the value and zero-fill the last two bytes. A target device address of all zeroes effectively addresses all devices on the local network
                dw.Write(header.TargetMacAddress); // target mac address - 0 means all devices
                dw.Write(new byte[] { 0, 0, 0, 0, 0, 0 }); //reserved 1

                //The client can use acknowledgements to determine that the LIFX device has received a message. 
                //However, when using acknowledgements to ensure reliability in an over-burdened lossy network ... 
                //causing additional network packets may make the problem worse. 
                //Client that don't need to track the updated state of a LIFX device can choose not to request a 
                //response, which will reduce the network burden and may provide some performance advantage. In
                //some cases, a device may choose to send a state update response independent of whether res_required is set.
                if (header.AcknowledgeRequired && header.ResponseRequired)
                    dw.Write((byte)0x03);
                else if (header.AcknowledgeRequired)
                    dw.Write((byte)0x02);
                else if (header.ResponseRequired)
                    dw.Write((byte)0x01);
                else
                    dw.Write((byte)0x00);
                //The sequence number allows the client to provide a unique value, which will be included by the LIFX 
                //device in any message that is sent in response to a message sent by the client. This allows the client
                //to distinguish between different messages sent with the same source identifier in the Frame. See
                //ack_required and res_required fields in the Frame Address.
                dw.Write((byte)header.Sequence);
                #endregion Frame address

                #region Protocol Header
                //The at_time value should be zero for Set and Get messages sent by a client.
                //For State messages sent by a device, the at_time will either be the device
                //current time when the message was received or zero. StateColor is an example
                //of a message that will return a non-zero at_time value
                if (header.AtTime > DateTime.MinValue)
                {
                    var time = header.AtTime.ToUniversalTime();
                    dw.Write((UInt64)(time - new DateTime(1970, 01, 01)).TotalMilliseconds * 10); //timestamp
                }
                else
                {
                    dw.Write((UInt64)0);
                }
                #endregion Protocol Header
                dw.Write(type); //packet _type
                dw.Write((UInt16)0); //reserved
                if (payload != null)
                    dw.Write(payload);
                dw.Flush();
            }
        }

        /// <summary>
		/// Turns the device on
		/// </summary>
		public Task TurnDeviceOnAsync(Device device)
        {
            System.Diagnostics.Debug.WriteLine("Sending TurnDeviceOn to {0}", device.HostName);
            return SetDevicePowerStateAsync(device, true);
        }
        /// <summary>
        /// Turns the device off
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public Task TurnDeviceOffAsync(Device device)
        {
            System.Diagnostics.Debug.WriteLine("Sending TurnDeviceOff to {0}", device.HostName);
            return SetDevicePowerStateAsync(device, false);
        }
        /// <summary>
        /// Sets the device power state
        /// </summary>
        /// <param name="device"></param>
        /// <param name="isOn"></param>
        /// <returns></returns>
        public async Task SetDevicePowerStateAsync(Device device, bool isOn)
        {
            System.Diagnostics.Debug.WriteLine("Sending TurnDeviceOff to {0}", device.HostName);
            FrameHeader header = new FrameHeader()
            {
                Identifier = (uint)randomizer.Next(),
                AcknowledgeRequired = true
            };

            await BroadcastMessageAsync<AcknowledgementResponse>(device.HostName, header,
                MessageType.DeviceSetPower, (UInt16)(isOn ? 65535 : 0));
        }

        /// <summary>
        /// Gets the label for the device
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public async Task<string> GetDeviceLabelAsync(Device device)
        {
            FrameHeader header = new FrameHeader()
            {
                Identifier = (uint)randomizer.Next(),
                AcknowledgeRequired = false
            };
            var resp = await BroadcastMessageAsync<StateLabelResponse>(device.HostName, header, MessageType.DeviceGetLabel);
            return resp.Label;
        }

        /// <summary>
        /// Sets the label on the device
        /// </summary>
        /// <param name="device"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        public async Task SetDeviceLabelAsync(Device device, string label)
        {
            FrameHeader header = new FrameHeader()
            {
                Identifier = (uint)randomizer.Next(),
                AcknowledgeRequired = true
            };
            var resp = await BroadcastMessageAsync<AcknowledgementResponse>(
                device.HostName, header, MessageType.DeviceSetLabel, label);
        }

        /// <summary>
        /// Gets the device version
        /// </summary>
        public async Task<StateVersionResponse> GetDeviceVersionAsync(Device device)
        {
            FrameHeader header = new FrameHeader()
            {
                Identifier = (uint)randomizer.Next(),
                AcknowledgeRequired = false
            };
            var resp = await BroadcastMessageAsync<StateVersionResponse>(device.HostName, header, MessageType.DeviceGetVersion);
            return resp;
        }
        /// <summary>
        /// Gets the device's host firmware
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public async Task<StateHostFirmwareResponse> GetDeviceHostFirmwareAsync(Device device)
        {
            FrameHeader header = new FrameHeader()
            {
                Identifier = (uint)randomizer.Next(),
                AcknowledgeRequired = false
            };
            var resp = await BroadcastMessageAsync<StateHostFirmwareResponse>(device.HostName, header, MessageType.DeviceGetHostFirmware);
            return resp;
        }

        private static Random randomizer = new Random();
        private UInt32 discoverSourceID;
        private CancellationTokenSource _DiscoverCancellationSource;
        private Dictionary<string, Device> DiscoveredBulbs = new Dictionary<string, Device>();

        /// <summary>
        /// Event fired when a LIFX bulb is discovered on the network
        /// </summary>
        public event EventHandler<DeviceDiscoveryEventArgs> DeviceDiscovered;
        /// <summary>
        /// Event fired when a LIFX bulb hasn't been seen on the network for a while (for more than 5 minutes)
        /// </summary>
        public event EventHandler<DeviceDiscoveryEventArgs> DeviceLost;

        private IList<Device> devices = new List<Device>();

        /// <summary>
        /// Gets a list of currently known devices
        /// </summary>
        public IEnumerable<Device> Devices { get { return devices; } }

        /// <summary>
        /// Event args for <see cref="DeviceDiscovered"/> and <see cref="DeviceLost"/> events.
        /// </summary>
        public sealed class DeviceDiscoveryEventArgs : EventArgs
        {
            /// <summary>
            /// The device the event relates to
            /// </summary>
            public Device Device { get; internal set; }
        }

        private void ProcessDeviceDiscoveryMessage(System.Net.IPAddress remoteAddress, int remotePort, LifxResponse msg)
        {
            string id = msg.Header.TargetMacAddressName; //remoteAddress.ToString()
            if (DiscoveredBulbs.ContainsKey(id))  //already discovered
            {
                DiscoveredBulbs[id].LastSeen = DateTime.UtcNow; //Update datestamp
                DiscoveredBulbs[id].HostName = remoteAddress.ToString(); //Update hostname in case IP changed

                return;
            }
            if (msg.Source != discoverSourceID || //did we request the discovery?
                _DiscoverCancellationSource == null ||
                _DiscoverCancellationSource.IsCancellationRequested) //did we cancel discovery?
                return;

            var device = new LightBulb()
            {
                HostName = remoteAddress.ToString(),
                Service = msg.Payload[0],
                Port = BitConverter.ToUInt32(msg.Payload, 1),
                LastSeen = DateTime.UtcNow,
                MacAddress = msg.Header.TargetMacAddress
            };
            DiscoveredBulbs[id] = device;
            devices.Add(device);
            if (DeviceDiscovered != null)
            {
                DeviceDiscovered(this, new DeviceDiscoveryEventArgs() { Device = device });
            }
        }

        /// <summary>
        /// Begins searching for bulbs.
        /// </summary>
        /// <seealso cref="DeviceDiscovered"/>
        /// <seealso cref="DeviceLost"/>
        /// <seealso cref="StopDeviceDiscovery"/>
        public void StartDeviceDiscovery()
        {
            if (_DiscoverCancellationSource != null && !_DiscoverCancellationSource.IsCancellationRequested)
                return;
            _DiscoverCancellationSource = new CancellationTokenSource();
            var token = _DiscoverCancellationSource.Token;
            var source = discoverSourceID = (uint)randomizer.Next(int.MaxValue);
            //Start discovery thread
            Task.Run(async () =>
            {
                System.Diagnostics.Debug.WriteLine("Sending GetServices");
                FrameHeader header = new FrameHeader()
                {
                    Identifier = source
                };
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        await BroadcastMessageAsync<UnknownResponse>(null, header, MessageType.DeviceGetService, null);
                    }
                    catch { }
                    await Task.Delay(5000);
                    var lostDevices = devices.Where(d => (DateTime.UtcNow - d.LastSeen).TotalMinutes > 5).ToArray();
                    if (lostDevices.Any())
                    {
                        foreach (var device in lostDevices)
                        {
                            devices.Remove(device);
                            DiscoveredBulbs.Remove(device.MacAddressName);
                            if (DeviceLost != null)
                                DeviceLost(this, new DeviceDiscoveryEventArgs() { Device = device });
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Stops device discovery
        /// </summary>
        /// <seealso cref="StartDeviceDiscovery"/>
        public void StopDeviceDiscovery()
        {
            if (_DiscoverCancellationSource == null || _DiscoverCancellationSource.IsCancellationRequested)
                return;
            _DiscoverCancellationSource.Cancel();
            _DiscoverCancellationSource = null;
        }

        private Dictionary<UInt32, Action<LifxResponse>> taskCompletions = new Dictionary<uint, Action<LifxResponse>>();

        /// <summary>
        /// Turns a bulb on using the provided transition time
        /// </summary>
        /// <param name="bulb"></param>
        /// <param name="transitionDuration"></param>
        /// <returns></returns>
        public Task TurnBulbOnAsync(LightBulb bulb, TimeSpan transitionDuration)
        {
            System.Diagnostics.Debug.WriteLine("Sending TurnBulbOn to {0}", bulb.HostName);
            return SetLightPowerAsync(bulb, transitionDuration, true);
        }
        /// <summary>
        /// Turns a bulb off using the provided transition time
        /// </summary>
        public Task TurnBulbOffAsync(LightBulb bulb, TimeSpan transitionDuration)
        {
            System.Diagnostics.Debug.WriteLine("Sending TurnBulbOff to {0}", bulb.HostName);
            return SetLightPowerAsync(bulb, transitionDuration, false);
        }
        private async Task SetLightPowerAsync(LightBulb bulb, TimeSpan transitionDuration, bool isOn)
        {
            if (bulb == null)
                throw new ArgumentNullException("bulb");
            if (transitionDuration.TotalMilliseconds > UInt32.MaxValue ||
                transitionDuration.Ticks < 0)
                throw new ArgumentOutOfRangeException("transitionDuration");

            FrameHeader header = new FrameHeader()
            {
                Identifier = (uint)randomizer.Next(),
                AcknowledgeRequired = true
            };

            var b = BitConverter.GetBytes((UInt16)transitionDuration.TotalMilliseconds);

            await BroadcastMessageAsync<AcknowledgementResponse>(bulb.HostName, header, MessageType.LightSetPower,
                (UInt16)(isOn ? 65535 : 0), b
            ).ConfigureAwait(false);
        }
        /// <summary>
        /// Gets the current power state for a light bulb
        /// </summary>
        /// <param name="bulb"></param>
        /// <returns></returns>
        public async Task<bool> GetLightPowerAsync(LightBulb bulb)
        {
            FrameHeader header = new FrameHeader()
            {
                Identifier = (uint)randomizer.Next(),
                AcknowledgeRequired = true
            };
            return (await BroadcastMessageAsync<LightPowerResponse>(
                bulb.HostName, header, MessageType.LightGetPower).ConfigureAwait(false)).IsOn;
        }

        /// <summary>
        /// Sets color and temperature for a bulb
        /// </summary>
        /// <param name="bulb"></param>
        /// <param name="color"></param>
        /// <param name="kelvin"></param>
        /// <returns></returns>
        public Task SetColorAsync(LightBulb bulb, Color color, UInt16 kelvin)
        {
            return SetColorAsync(bulb, color, kelvin, TimeSpan.Zero);
        }
        /// <summary>
        /// Sets color and temperature for a bulb and uses a transition time to the provided state
        /// </summary>
        /// <param name="bulb"></param>
        /// <param name="color"></param>
        /// <param name="kelvin"></param>
        /// <param name="transitionDuration"></param>
        /// <returns></returns>
        public Task SetColorAsync(LightBulb bulb, Color color, UInt16 kelvin, TimeSpan transitionDuration)
        {
            var hsl = Utilities.RgbToHsl(color);
            return SetColorAsync(bulb, hsl[0], hsl[1], hsl[2], kelvin, transitionDuration);
        }

        /// <summary>
        /// Sets color and temperature for a bulb and uses a transition time to the provided state
        /// </summary>
        /// <param name="bulb">Light bulb</param>
        /// <param name="hue">0..65535</param>
        /// <param name="saturation">0..65535</param>
        /// <param name="brightness">0..65535</param>
        /// <param name="kelvin">2700..9000</param>
        /// <param name="transitionDuration"></param>
        /// <returns></returns>
        public async Task SetColorAsync(LightBulb bulb,
            UInt16 hue,
            UInt16 saturation,
            UInt16 brightness,
            UInt16 kelvin,
            TimeSpan transitionDuration)
        {
            if (transitionDuration.TotalMilliseconds > UInt32.MaxValue ||
                transitionDuration.Ticks < 0)
                throw new ArgumentOutOfRangeException("transitionDuration");
            if (kelvin < 2500 || kelvin > 9000)
            {
                throw new ArgumentOutOfRangeException("kelvin", "Kelvin must be between 2500 and 9000");
            }

            System.Diagnostics.Debug.WriteLine("Setting color to {0}", bulb.HostName);
            FrameHeader header = new FrameHeader()
            {
                Identifier = (uint)randomizer.Next(),
                AcknowledgeRequired = true
            };
            UInt32 duration = (UInt32)transitionDuration.TotalMilliseconds;
            var durationBytes = BitConverter.GetBytes(duration);
            var h = BitConverter.GetBytes(hue);
            var s = BitConverter.GetBytes(saturation);
            var b = BitConverter.GetBytes(brightness);
            var k = BitConverter.GetBytes(kelvin);

            await BroadcastMessageAsync<AcknowledgementResponse>(bulb.HostName, header,
                MessageType.LightSetColor, (byte)0x00, //reserved
                    hue, saturation, brightness, kelvin, //HSBK
                    duration
            );
        }

        /*
		public async Task SetBrightnessAsync(LightBulb bulb,
			UInt16 brightness,
			TimeSpan transitionDuration)
		{
			if (transitionDuration.TotalMilliseconds > UInt32.MaxValue ||
				transitionDuration.Ticks < 0)
				throw new ArgumentOutOfRangeException("transitionDuration");

			FrameHeader header = new FrameHeader()
			{
				Identifier = (uint)randomizer.Next(),
				AcknowledgeRequired = true
			};
			UInt32 duration = (UInt32)transitionDuration.TotalMilliseconds;
			var durationBytes = BitConverter.GetBytes(duration);
			var b = BitConverter.GetBytes(brightness);

			await BroadcastMessageAsync<AcknowledgementResponse>(bulb.HostName, header,
				MessageType.SetLightBrightness, brightness, duration
			);
		}*/

        /// <summary>
        /// Gets the current state of the bulb
        /// </summary>
        /// <param name="bulb"></param>
        /// <returns></returns>
        public Task<LightStateResponse> GetLightStateAsync(LightBulb bulb)
        {
            FrameHeader header = new FrameHeader()
            {
                Identifier = (uint)randomizer.Next(),
                AcknowledgeRequired = false
            };
            return BroadcastMessageAsync<LightStateResponse>(
                bulb.HostName, header, MessageType.LightGet);
        }
    }
}