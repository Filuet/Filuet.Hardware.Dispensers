using System;
using System.Net;

namespace Filuet.Hardware.Dispensers.SDK.Jofemar.VisionEsPlus.Communication
{
    public class EspSerialChannelSettings
    {
        public string PortName;
        public TimeSpan ReadDelay = TimeSpan.FromMilliseconds(200);
        public TimeSpan ReceiveTimeout = TimeSpan.FromSeconds(2);
    }
}
