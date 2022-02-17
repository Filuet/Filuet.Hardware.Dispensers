using Filuet.Hardware.Dispensers.Abstractions;
using Filuet.Hardware.Dispensers.Abstractions.Enums;
using Filuet.Hardware.Dispensers.SDK.Jofemar.VisionEsPlus.Enums;
using Filuet.Hardware.Dispensers.SDK.Jofemar.VisionEsPlus.Models;
using Filuet.Infrastructure.Abstractions.Helpers;
using Filuet.Infrastructure.Communication;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Filuet.Hardware.Dispensers.SDK.Jofemar.VisionEsPlus
{
    public class VisionEsPlus
    {
        public event EventHandler<DispenseEventArgs> onDispensing;
        public event EventHandler<string> onStatus;
        private VisionEsPlusSettings _settings;
        private readonly byte[] _commandBody;
        private ICommunicationChannel _channel;

        public VisionEsPlus(ICommunicationChannel channel, VisionEsPlusSettings settings)
        {
            _settings = settings;
            byte machineAddress = (byte)(Byte.Parse(_settings.Address.Substring(2), NumberStyles.HexNumber) + 0x80);
            _commandBody = new byte[] { 0x02 /* start of the message */,
                0x30 /* Filler1 */,
                0x30 /* Filler2 */,
                machineAddress /* Machine address (1-31) 0x81*/,
                0 /* Type */,
                0xff /* Param1 */,
                0xff /* Param2 */,
                0 /* CheckSum1 */,
                0  /* CheckSum1 */,
                0x03  /* End of message */ };
            _channel = channel;
        }

        #region Actions
        internal void ChangeLight(bool isOn)
        {
            _settings.LightSettings.LightIsOn = isOn;
            //// OnSettingsChanged.Invoke
            _channel.SendCommand(ChangeLightCommand(isOn), _settings.CommandReadDelay, _settings.Timeout);
        }

        internal void Blink(TimeSpan? duration = null)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            bool lightIsOn = _settings.LightSettings.LightIsOn;
            while (sw.Elapsed < duration)
            {
                _channel.SendCommand(ChangeLightCommand(!lightIsOn), _settings.CommandReadDelay, _settings.Timeout);
                lightIsOn = !lightIsOn;
                Thread.Sleep(_settings.LightSettings.BlinkingPeriod);
            }
            sw.Stop();
        }

        internal void Reset()
        {
            lock (_channel)
            {
                _channel.SendCommand(ResetCommand(), _settings.CommandReadDelay, _settings.Timeout);
            }
        }

        internal async Task<(DispenserStateSeverity state, VisionEsPlusResponseCodes internalState, string message)?> Status()
            => await Task.Factory.StartNew<(DispenserStateSeverity state, VisionEsPlusResponseCodes internalState, string message)?>(() =>
            {
                byte[] response = null;

                try
                {
                    response = _channel.SendCommand(StatusCommand(), _settings.CommandReadDelay, _settings.Timeout);
                }
                catch (SocketException)
                { }

                if (response == null || response.Length == 0)
                {
                    onStatus?.Invoke(this, "No response");
                    return (DispenserStateSeverity.Inoperable, VisionEsPlusResponseCodes.Unknown, "The machine is inoperable");
                }

                onStatus?.Invoke(this, BitConverter.ToString(response).Replace("-", string.Empty));

                VisionEsPlusResponseCodes code = ParseResponse(response);

                switch (code)
                {
                    case VisionEsPlusResponseCodes.Unknown:
                        return (DispenserStateSeverity.Inoperable, code, "The machine is inoperable");
                    case VisionEsPlusResponseCodes.Ok:
                    case VisionEsPlusResponseCodes.Ready:
                    case VisionEsPlusResponseCodes.FaultIn485Bus:
                        return (DispenserStateSeverity.Normal, code, "The machine is connected");
                    default:
                        switch (TryGetDoorState(response))
                        {
                            case DoorState.DoorClosed:
                                return (DispenserStateSeverity.Normal, code, "The door was closed");
                            case DoorState.DoorOpen:
                                return (DispenserStateSeverity.MaintenanceService, code, "The door is open");
                            case DoorState.Unknown:
                            default:
                                return (DispenserStateSeverity.Inoperable, code, "The machine is inoperable");
                        }
                }
            });

        internal async Task MultiplyDispensing(EspBeltAddress address, uint quantity)
        {
            Dictionary<EspBeltAddress, uint> map = new Dictionary<EspBeltAddress, uint>();
            map.Add(address, quantity);

            await MultiplyDispensing(map);
        }

        internal async Task MultiplyDispensing(IDictionary<EspBeltAddress, uint> map)
        {
            (DispenserStateSeverity state, VisionEsPlusResponseCodes internalState, string message)? state = await Status();

            if (state?.state != DispenserStateSeverity.Normal)
                return;

            state = null;

            foreach (var a in map)
            {
                for (uint i = 1; i <= a.Value; i++)
                {
                    Dispense(a.Key, a.Key == map.Last().Key && i == a.Value/*Send VEND command if last element*/);

                    while (state == null)
                    {
                        state = await Status(); // Wait for the next not empty state (it means state of the dispensing command) 
                    }

                    if (state?.state == DispenserStateSeverity.Normal) // If product was dispensed successfully
                    {
                        onDispensing?.Invoke(this, DispenseEventArgs.Create(a.Key));
                        // Check if the machine ready to handle next command
                        state = null;
                        state = await Status();

                        if (state?.state != DispenserStateSeverity.Normal) // The machine is in error state
                        {
                            //....
                        }
                    }
                }
            }

            // Up to this moment we have deispensed products. Now we must send the elevator to the parking lot:
            ////////////_channel.SendCommand(ParkingCommand(), _settings.CommandReadDelay, _settings.Timeout);
            // Send status until receive 0x4F (W. F. P. TO BE REMOVED)
            // Send status until receive 0x06 0x30 (Ready) Standby mode that mean the products have been dispensed
        }

        private void Dispense(EspBeltAddress address, bool lastCommand)
        {
            byte[] response = _channel.SendCommand(DispenseCommand(address.Tray, address.Belt, lastCommand), _settings.CommandReadDelay, _settings.Timeout);
                //VisionEsPlusResponseCodes code = ParseResponse(response);
        }

        internal bool IsBeltAvailable(uint machineId, string route)
        {
            EspBeltAddress address = route;

            if (address.Machine != machineId) // The route belongs to another machine
                return false;

            byte[] response = _channel.SendCommand(CheckChannelCommand(address.Tray, address.Belt), _settings.CommandReadDelay, _settings.Timeout);

            return response.Length == 8 && response[4] == 0x43; // 0x44 means bealt is unavailable
        }

        public DoorState TryGetDoorState(byte[] arr)
        {
            if (arr == null || arr.Length != 7 || arr[2] != 0x50)
                return DoorState.Unknown;
            var doorState = (DoorState)arr[3];
            return doorState;
        }
        #endregion

        #region Commands
        /// <summary>
        /// Turn on/off the light
        /// </summary>
        /// <returns>команда</returns>
        private byte[] ChangeLightCommand(bool isOn)
        {
            var comm = new byte[_commandBody.Length];
            Array.Copy(_commandBody, comm, _commandBody.Length);
            comm[4] = 0x4c;
            if (isOn)
                comm[5] = 0x81;
            else comm[5] = 0x80;
            InjectCheckSumm(comm);
            return comm;
        }

        /// <summary>
        /// Get status of dispensing machine
        /// </summary>
        /// <returns>команда</returns>
        private byte[] StatusCommand()
        {
            var comm = new byte[_commandBody.Length];
            Array.Copy(_commandBody, comm, _commandBody.Length);
            comm[4] = 0x53;
            InjectCheckSumm(comm);
            return comm;
        }

        /// <summary>
        /// Get status of dispensing machine
        /// </summary>
        /// <returns>команда</returns>
        private byte[] CheckChannelCommand(int tray, int belt)
        {
            var comm = new byte[_commandBody.Length];
            Array.Copy(_commandBody, comm, _commandBody.Length);
            comm[4] = 0x43;
            comm[5] = 0x43;
            comm[6] = (byte)(tray * 10 + belt);
            InjectCheckSumm(comm);
            return comm;
        }

        /// <summary>
        /// Extracting with parking
        /// </summary>
        /// <param name="tray"></param>
        /// <param name="belt"></param>
        /// <returns></returns>
        private byte[] DispenseCommand(int tray, int belt, bool lastItem)
        {
            var retval = new byte[_commandBody.Length];
            Array.Copy(_commandBody, retval, _commandBody.Length); 
            retval[4] = (byte)(lastItem ? 0x56 : 0x4D); // Vend command : Multiply dispense command
            retval[5] = (byte)(tray + 0x80);
            retval[6] = (byte)(belt + 0x80);
            InjectCheckSumm(retval);
            return retval;
        }

        /// <summary>
        /// Parks the lift is anything to extract
        /// </summary>
        /// <returns></returns>
        private byte[] ParkingCommand()
        {
            var retval = new byte[_commandBody.Length];
            Array.Copy(_commandBody, retval, _commandBody.Length);
            retval[4] = 0x4d;
            retval[5] = 0x80;
            retval[6] = 0x80;
            InjectCheckSumm(retval);
            return retval;
        }

        /// <summary>
        /// Resets the machine
        /// </summary>
        /// <returns></returns>
        private byte[] ResetCommand()
        {
            var retval = new byte[_commandBody.Length];
            Array.Copy(_commandBody, retval, _commandBody.Length);
            retval[4] = 0x52;
            InjectCheckSumm(retval);
            return retval;
        }
        #endregion

        #region Private
        /// <summary>
        /// Вставляет во второй и третий с конца байты контрольную сумму в соответствии с протоколом
        /// </summary>
        /// <param name="message">сообщение</param>
        private void InjectCheckSumm(IList<byte> message)
        {
            var summ = 0;
            for (var i = 0; i < message.Count - 3; i++)
                summ += message[i];
            summ &= 0xff;
            message[message.Count - 3] = (byte)(summ | 0xf0);
            message[message.Count - 2] = (byte)(summ | 0x0f);
        }

        /// <summary>
        /// Takes last byte from response array and converts it to <see cref="VisionEsPlusResponseCodes" />
        /// </summary>
        /// <param name="response"></param>
        private VisionEsPlusResponseCodes ParseResponse(IReadOnlyList<byte> response)
            => response == null || (response != null && (response.Count > 10 || response.Count == 0)) ?
                VisionEsPlusResponseCodes.Unknown : (VisionEsPlusResponseCodes)response[response.Count - 1];
        #endregion
    }
}