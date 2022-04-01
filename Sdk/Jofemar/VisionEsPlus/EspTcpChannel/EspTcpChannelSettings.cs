using System;
using System.Net;

namespace Filuet.Hardware.Dispensers.SDK.Jofemar.VisionEsPlus.Communication
{
    public class EspTcpChannelSettings
    {

        public IPEndPoint Endpoint;
        public TimeSpan ReadDelay = TimeSpan.FromMilliseconds(200);
        public TimeSpan ReceiveTimeout = TimeSpan.FromSeconds(20);
    }
}
