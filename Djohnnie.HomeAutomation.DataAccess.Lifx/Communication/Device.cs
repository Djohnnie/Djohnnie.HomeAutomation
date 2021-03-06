﻿using System;
using System.Linq;

namespace Djohnnie.HomeAutomation.DataAccess.Lifx.Communication
{
    /// <summary>
	/// LIFX Generic Device
	/// </summary>
	public abstract class Device
    {
        internal Device() { }
        /// <summary>
        /// Hostname for the device
        /// </summary>
        public string HostName { get; internal set; }
        /// <summary>
        /// Service ID
        /// </summary>
        public byte Service { get; internal set; }
        /// <summary>
        /// Service port
        /// </summary>
        public UInt32 Port { get; internal set; }
        internal DateTime LastSeen { get; set; }
        /// <summary>
        /// Gets the MAC address
        /// </summary>
        public byte[] MacAddress { get; internal set; }
        /// <summary>
        /// Gets the MAC address
        /// </summary>
        public string MacAddressName
        {
            get
            {
                if (MacAddress == null) return null;
                return string.Join(":", MacAddress.Take(6).Select(tb => tb.ToString("X2")).ToArray());
            }
        }
    }
}