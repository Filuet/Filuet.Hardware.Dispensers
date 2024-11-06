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
        public string IpOrSerialAddress { get; set; } // for testing purposes and remote control
        public ushort PortNumber { get; set; }
        public ushort BaudRate { get; set; } = 9600;
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(20);
        /// <summary>
        /// How often should we send Status request
        /// </summary>
        public decimal PollFrequencyHz { get; set; } = 0.5m;
        public VisionEsPlusLightEmitterSettings LightSettings { get; set; } = new VisionEsPlusLightEmitterSettings();
        /// <summary>
        /// Max extraction weight per time
        /// </summary>
        public int MaxExtractWeightPerTime { get; set; } = 3500;

        public string ID
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Alias))
                    return Alias;

                if (!string.IsNullOrWhiteSpace(IpOrSerialAddress))
                    return $"{IpOrSerialAddress}:{PortNumber}";

                return Address;
            }
        }
    }
}