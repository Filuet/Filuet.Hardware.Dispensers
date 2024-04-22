using Filuet.Hardware.Dispensers.Abstractions;
using Filuet.Hardware.Dispensers.Abstractions.Enums;
using Filuet.Hardware.Dispensers.SDK.Jofemar.VisionEsPlus.Enums;
using Filuet.Hardware.Dispensers.SDK.Jofemar.VisionEsPlus.Models;
using Filuet.Infrastructure.Communication;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Filuet.Hardware.Dispensers.SDK.Jofemar.VisionEsPlus
{
    public class VisionEsPlus
    {
        public event EventHandler<bool> onLightsChanged;
        public event EventHandler<DispenseEventArgs> onDispensing;
        public event EventHandler<DispenseEventArgs> onDispensed;
        public event EventHandler<DispenseEventArgs> onWaitingProductsToBeRemoved;
        /// <summary>
        /// Fires when the customer forget to pick up the products
        /// </summary>
        public event EventHandler<DispenseEventArgs> onAbandonment;

        /// <summary>
        /// tracks data transmitting hither and thither: 1) app -> dispenser; 2) dispenser -> app
        /// 'true' means command, 'false' means response
        /// </summary>
        public event EventHandler<(bool direction, string message, string data)> onDataMoving;

        public VisionEsPlus(ICommunicationChannel channel, VisionEsPlusSettings settings)
        {
            _settings = settings;
            byte machineAddress = (byte)(byte.Parse(_settings.Address.Substring(2), NumberStyles.HexNumber) + 0x80);
            _commandBody = new byte[] { 0x02 /* start of the message */, 0x30 /* Filler1 */, 0x30 /* Filler2 */,
                machineAddress /* Machine address (1-31) 0x81*/, 0 /* Type */, 0xff /* Param1 */, 0xff /* Param2 */, 0 /* CheckSum1 */, 0  /* CheckSum1 */, 0x03  /* End of message */ };
            _channel = channel;

            ChangeLight(settings.LightSettings.LightsAreNormallyOn);
        }

        #region Actions
        internal void ChangeLight(bool isOn)
        {
            ExecuteCommand(ChangeLightCommand(isOn), $"Lights {(isOn ? "on" : "off")}", code =>
            {
                if (code == VisionEsPlusResponseCodes.Ok)
                    onLightsChanged?.Invoke(this, isOn); // Send an event to the business layer
            });
        }

        internal void Reset()
        {
            ExecuteCommand(ResetCommand(), "Reset");
        }

        internal void Unlock()
        {
            ExecuteCommand(UnlockTheDoor(), "Unlock");
        }

        internal async Task<(DispenserStateSeverity state, VisionEsPlusResponseCodes internalState, string message)?> Status()
            => await Task.Factory.StartNew<(DispenserStateSeverity state, VisionEsPlusResponseCodes internalState, string message)?>(() =>
                ExecuteCommand(StatusCommand(), "Status", (response, code) =>
                {
                    switch (code)
                    {
                        case VisionEsPlusResponseCodes.Unknown:
                            return (DispenserStateSeverity.Inoperable, code, "inoperable");
                        case VisionEsPlusResponseCodes.Ok:
                        case VisionEsPlusResponseCodes.Ready:
                        case VisionEsPlusResponseCodes.FaultIn485Bus:
                            return (DispenserStateSeverity.Normal, code, "connected");
                        default:
                            switch (TryGetDoorState(response))
                            {
                                case DoorState.DoorClosed:
                                    return (DispenserStateSeverity.Normal, code, "the door was closed");
                                case DoorState.DoorOpen:
                                    return (DispenserStateSeverity.MaintenanceService, code, "the door is open");
                                case DoorState.Unknown:
                                default:
                                    return (DispenserStateSeverity.Inoperable, code, "inoperable");
                            }
                    }
                }));

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

            ChangeLight(true); // Turn on lights before dispensing 

            List<DispenseEventArgs> droppedIntoTheElevatorProducts = new List<DispenseEventArgs>();

            foreach (var a in map)
            {
                for (uint i = 1; i <= a.Value; i++)
                {
                    state = null;

                    onDispensing?.Invoke(this, DispenseEventArgs.Create(a.Key));
                    Dispense(a.Key, a.Key == map.Last().Key && i == a.Value/*Send VEND command if last element*/);

                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    while ((state == null || state.Value.state == DispenserStateSeverity.Inoperable) && sw.Elapsed.TotalSeconds < 30)
                    {
                        try
                        {
                            Thread.Sleep(3000);
                            state = await Status(); // Wait for the next not empty state
                        }
                        catch (SocketException)
                        { }
                        catch (Exception ex)
                        { }
                    }
                    sw.Stop();

                    switch (state?.state)
                    {
                        case DispenserStateSeverity.Normal: // the product was extracted from the belt to the elevator successfully
                            droppedIntoTheElevatorProducts.Add(DispenseEventArgs.Create(a.Key));

                            state = null;
                            state = await Status(); // Check if the machine ready to handle next command

                            if (state?.state != DispenserStateSeverity.Normal) // The machine is in error state
                            {
                                switch (state?.internalState)
                                {
                                    case VisionEsPlusResponseCodes.WaitingForProductToBeRemoved:
                                        onWaitingProductsToBeRemoved?.Invoke(this, DispenseEventArgs.Create(a.Key));
                                        state = null;

                                        Stopwatch sw1 = new Stopwatch();
                                        sw1.Start();

                                        while ((state == null ||
                                            state.Value.state == DispenserStateSeverity.Inoperable ||
                                            state.Value.internalState == VisionEsPlusResponseCodes.WaitingForProductToBeRemoved) && sw1.Elapsed.TotalSeconds < 120)
                                        {
                                            try
                                            {
                                                Thread.Sleep(3000);
                                                state = await Status(); // Wait for the next not empty state 
                                            }
                                            catch (SocketException)
                                            { }
                                            catch (Exception ex)
                                            { }
                                        }
                                        sw1.Stop();

                                        if (state?.internalState == VisionEsPlusResponseCodes.Ready || state?.internalState == VisionEsPlusResponseCodes.FaultIn485Bus) // The product/s was/were given
                                        {
                                            foreach (var p in droppedIntoTheElevatorProducts)
                                                onDispensed?.Invoke(this, p);

                                            droppedIntoTheElevatorProducts.Clear(); // Consider the products as dispensed
                                        }
                                        else if (state?.internalState == VisionEsPlusResponseCodes.WaitingForProductToBeRemoved)
                                        {
                                            // Looks like the customer has forgotten to pick up the products. At least they're in the elevator so far
                                            foreach (var p in droppedIntoTheElevatorProducts)
                                                onAbandonment?.Invoke(this, p);

                                            droppedIntoTheElevatorProducts.Clear(); // Consider the products as disputable, but as for now forget them
                                        }
                                        else if (state?.internalState == VisionEsPlusResponseCodes.FaultInProductDetector)
                                        {
                                            // This could happen when during the descent the product fell and stopped blocking the detector or, conversely, turned over and blocked it.
                                            // This doesn't mean that the product wasn't issued
                                        }

                                        break;
                                    default:
                                        break;
                                }
                            }

                            break;
                        default:
                            break;
                    }
                }
            }

            // Turn off lights after dispensing
            if (!_settings.LightSettings.LightsAreNormallyOn)
                ChangeLight(false);
        }

        private void Dispense(EspBeltAddress address, bool lastCommand)
        {
            ExecuteCommand(DispenseCommand(address.Tray, address.Belt, lastCommand), "Dispense");
        }

        internal bool? IsBeltActive(uint machineId, string route)
        {
            EspBeltAddress address = route;

            if (address.Machine != machineId) // The route belongs to another machine
                return null;

            return ExecuteCommand(CheckChannelCommand(address.Tray, address.Belt), "CheckBelt", (response, code) =>
                response.Length == 8 && response[4] == 0x43); // 0x44 means bealt is unavailable
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

        /// <summary>
        /// Unlock the elevator door
        /// </summary>
        /// <returns></returns>
        private byte[] UnlockTheDoor()
        {
            var retval = new byte[_commandBody.Length];
            Array.Copy(_commandBody, retval, _commandBody.Length);
            retval[4] = 0x4E;
            InjectCheckSumm(retval);
            return retval;
        }
        #endregion

        #region Private
        private DoorState TryGetDoorState(byte[] arr)
        {
            if (arr == null || arr.Length != 7 || arr[2] != 0x50)
                return DoorState.Unknown;
            var doorState = (DoorState)arr[3];
            return doorState;
        }

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

        private void ExecuteCommand(byte[] command, string message, Action<VisionEsPlusResponseCodes> postAction = null)
        {
            lock (_channel)
            {
                byte[] response = null;

                try
                {
                    onDataMoving?.Invoke(this, (true, $"{message} {_settings.ID}", _bitConvert(command))); // Send telemetry to subscribers via DispenserNegotialorLogger
                    response = _channel.SendCommand(command); // Send command to the device
                }
                catch (SocketException) { }

                VisionEsPlusResponseCodes code = ParseResponse(response);

                postAction?.Invoke(code);

                onDataMoving?.Invoke(this, (false, $"{message} {_settings.ID}: {code}", _bitConvert(response)));
            }
        }

        private T ExecuteCommand<T>(byte[] command, string message, Func<byte[], VisionEsPlusResponseCodes, T> postAction)
        {
            lock (_channel)
            {
                byte[] response = null;

                try
                {
                    onDataMoving?.Invoke(this, (true, $"{message} {_settings.ID}", _bitConvert(command))); // Send telemetry to subscribers via DispenserNegotialorLogger
                    response = _channel.SendCommand(command); // Send command to the device
                }
                catch (SocketException) { }

                VisionEsPlusResponseCodes code = ParseResponse(response);

                onDataMoving?.Invoke(this, (false, $"{message} {_settings.ID}: {code}", _bitConvert(response)));

                return postAction.Invoke(response, code);
            }
        }

        private string _bitConvert(byte[] data) => BitConverter.ToString(data).Replace('-', ' ');
        #endregion

        internal string Alias => _settings.Alias;

        private VisionEsPlusSettings _settings;
        private readonly byte[] _commandBody;
        private ICommunicationChannel _channel;
    }
}