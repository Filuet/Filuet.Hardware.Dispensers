using Filuet.Hardware.Dispensers.Common.Enums;
using Filuet.Hardware.Dispensers.Common.Interfaces;
using System;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace Filuet.Hardware.Dispensers.Common
{
    public class SerialPortChannel : ICommunicationChannel
    {
        private SerialPort _port;

        public SerialPortChannel(UInt16 serialPortNumber, UInt16 baudRate, TimeSpan timeout, TimeSpan commandsSendDelay)
        {
            _serialPortNumber = serialPortNumber;
            _baudRate = baudRate;
            _timeout = timeout;
            _commandsSendDelay = commandsSendDelay;

            if (_port == null)
                _port = new SerialPort($"COM{_serialPortNumber}", _baudRate);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data">command</param>
        /// <returns></returns>
        public byte[] SendCommand(byte[] data)
        {
            lock (_port)
            {
                var count = 0;
                while (true)
                {
                    var result = Write(data);
                    if (result != PortStateCode.Success)
                        return null; //throw new ExternalException($"Error in [{Port.Name}] write command [{command.ByteArrayToString()}]");
                    Thread.Sleep(_commandsSendDelay);

                    byte[] buff;
                    var response = Read(out buff);
                    if (response == PortStateCode.Success)
                        return buff;

                    if (count > (_timeout.TotalMilliseconds / _commandsSendDelay.TotalMilliseconds))
                        return null; //throw new TimeoutException($"Timeout in read answer on command [{command.ByteArrayToString()}]");

                    Thread.Sleep(_commandsSendDelay);
                    count++;
                }
            }
        }

        /// <summary>
        ///     Упаковывает и записывает в порт команду
        /// </summary>
        /// <param name="command">Команда</param>
        /// <returns>Результат записи в порт</returns>
        private PortStateCode Write(byte[] command)
        {
            try
            {
                if (!_port.IsOpen)
                    _port.Open();
                if (!_port.IsOpen)
                    return PortStateCode.PortClosed;

                _port.Write(command, 0, command.Length);

                Thread.Sleep(_commandsSendDelay);

                Console.WriteLine($"[{_port.PortName}] {Encoding.Default.GetString(command)}");
                return PortStateCode.Success;
            }
            catch (TimeoutException)
            {
                Console.WriteLine($"[{_port.PortName}] timeout");
                return PortStateCode.Timeout;
            }
            catch (Exception ex) when (ex is ArgumentNullException || ex is ArgumentOutOfRangeException || ex is ArgumentException)
            {
                Console.WriteLine($"[{_port.PortName}] invalid command");
                return PortStateCode.Failure;
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine($"[{_port.PortName}] port closed");
                return PortStateCode.PortClosed;
            }
        }

        /// <summary>
        ///     Считывает из порта пакет данных
        /// </summary>
        /// <returns></returns>
        private PortStateCode Read(out byte[] buffer)
        {
            if (!_port.IsOpen)
                _port.Open();
            if (!_port.IsOpen)
            {
                buffer = null;
                return PortStateCode.PortClosed;
            }

            byte[] data = new byte[_port.BytesToRead];

            try
            {
                _port.Read(data, 0, data.Length);
                buffer = data;
                Console.WriteLine($"[{_port.PortName}] {(buffer?.Length > 0 ? Encoding.Default.GetString(buffer) : string.Empty)}");
                return PortStateCode.Success;
            }
            catch (TimeoutException)
            {
                Console.WriteLine($"[{_port.PortName}] timeout");
                buffer = null;
                return PortStateCode.Timeout;
            }
            catch (Exception ex) when (ex is ArgumentNullException || ex is ArgumentOutOfRangeException || ex is ArgumentException)
            {
                Console.WriteLine($"[{_port.PortName}] invalid command");
                buffer = null;
                return PortStateCode.Failure;
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine($"[{_port.PortName}] port closed");
                buffer = null;
                return PortStateCode.PortClosed;
            }
        }

        private byte _address { get; set; } = 0x01;
        private ushort _serialPortNumber { get; set; }
        private ushort _baudRate { get; set; } = 9600;
        private TimeSpan _timeout { get; set; } = TimeSpan.FromSeconds(2);
        private TimeSpan _commandsSendDelay { get; set; } = TimeSpan.FromSeconds(0.2);
    }
}
