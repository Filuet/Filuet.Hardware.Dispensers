using System;

namespace Filuet.Hardware.Dispensers.SDK.Jofemar.VisionEsPlus
{
    public class VisionEsPlusSettings
    {
        public string Alias { get; set; }
        /// <summary>
        /// TCP: Ip address; Serial: 0x01 by default (all machines have 0x01 address cause we're separating them by serial ports)
        /// </summary>
        public string Address { get; set; } //= 0x01;
        public string IpAddress { get; set; } // for testing purposes and remote control
        public UInt16 PortNumber { get; set; }
        public UInt16 BaudRate { get; set; } = 9600;
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(20);
        public VisionEsPlusLightEmitterSettings LightSettings { get; set; } = new VisionEsPlusLightEmitterSettings();

        public string ID
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Alias))
                    return Alias;

                if (!string.IsNullOrWhiteSpace(IpAddress))
                    return $"{IpAddress}:{PortNumber}";

                return Address;
            }
        }
    }
}