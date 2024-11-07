using Filuet.Infrastructure.Abstractions.Helpers;
using Filuet.Infrastructure.Communication;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;

namespace Filuet.Hardware.Dispensers.SDK.Jofemar.VisionEsPlus.Communication
{
    public class EspSerialChannel : ICommunicationChannel
    {
        public EspSerialChannel(Action<EspSerialChannelSettings> channelSetup)
        {
            _settings = channelSetup.CreateTargetAndInvoke();
            _port = new SerialPort(_settings.PortName, 9600);
            _port.ReadTimeout = (int)_settings.ReceiveTimeout.TotalMilliseconds;
        }

        public byte[] SendCommand(byte[] data)
        {
            if (!_port.IsOpen)
            {
                try
                {
                    _port.Open();
                }
                catch (System.IO.FileNotFoundException)
                {
                    return new byte[] { };
                }
            }

            _port.Write(data, 0, data.Length);

            List<byte> bytes = new List<byte>();

            for (int i = 0; i < _settings.ReceiveTimeout.TotalMilliseconds / _settings.ReadDelay.TotalMilliseconds; i++)
            {
                Thread.Sleep(_settings.ReadDelay);
                byte[] block = new byte[_port.BytesToRead];
                _port.Read(block, 0, _port.BytesToRead);
                bytes.AddRange(block);
                if (block.Length == 0)
                    break;
            }

            return bytes.ToArray();
        }

        private readonly EspSerialChannelSettings _settings;
        private readonly SerialPort _port;
    }
}
