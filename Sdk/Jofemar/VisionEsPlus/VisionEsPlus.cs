using Filuet.Hardware.Dispensers.Abstractions;
using Filuet.Hardware.Dispensers.Abstractions.Enums;
using Filuet.Hardware.Dispensers.Abstractions.Models;
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
        /// <summary>
        /// start dispense
        /// </summary>
        public event EventHandler<DispenseEventArgs> onDispensing;
        /// <summary>
        /// product dispensed
        /// </summary>
        public event EventHandler<DispenseEventArgs> onDispensed;
        public event EventHandler<IEnumerable<DispenseEventArgs>> onWaitingProductsToBeRemoved;
        /// <summary>
        /// Fires when the customer forget to pick up the products
        /// </summary>
        public event EventHandler<DispenseEventArgs> onAbandonment;
        public event EventHandler<FailedToDispenseEventArgs> onFailedToDispense;
        public event EventHandler<DispenseFailedEventArgs> onAddressUnavailable;
        /// <summary>
        /// tracks data transmitting hither and thither: 1) app -> dispenser; 2) dispenser -> app
        /// 'true' means command, 'false' means response
        /// </summary>
        public event EventHandler<(bool direction, string message, string data)> onDataMoving;

        public int Id => _settings.Id;

        public VisionEsPlus(ICommunicationChannel channel, VisionEsPlusSettings settings, Func<Pog> getPlanogram, VisionEsPlusEmulationCache emulatorCache = null) {
            _settings = settings;
            _getPlanogram = getPlanogram;
            _emulatorCache = emulatorCache;

            if (_getPlanogram == null)
                throw new ArgumentNullException("Planogram is mandatory");

            _planogram = _getPlanogram?.Invoke();

            byte machineAddress = (byte)(byte.Parse(_settings.Address.Substring(2), NumberStyles.HexNumber) + 0x80);
            _commandBody = new byte[] { 0x02 /* start of the message */, 0x30 /* Filler1 */, 0x30 /* Filler2 */,
                machineAddress /* Machine address (1-31) 0x81*/, 0 /* Type */, 0xff /* Param1 */, 0xff /* Param2 */, 0 /* CheckSum1 */, 0  /* CheckSum1 */, 0x03  /* End of message */ };
            _channel = channel;

            ChangeLight(settings.LightSettings.LightsAreNormallyOn);
        }

        #region Actions
        internal void ChangeLight(bool isOn) {
            if (_settings.Emulation) {
                onLightsChanged?.Invoke(this, isOn);
                return;
            }

            runCommand(ChangeLightCommand(isOn), $"Lights are {(isOn ? "on" : "off")}", code => {
                if (code == VisionEsPlusResponseCodes.Ok)
                    onLightsChanged?.Invoke(this, isOn); // Send an event to the business layer
            });
        }

        internal void Reset() {
            if (_settings.Emulation) {
                _emulatorCache.InvokeReboot();
                onDataMoving?.Invoke(this, (true, $"Reboot Machine {Id}", "emulated"));
                return;
            }

            runCommand(ResetCommand(), "Reset");
        }

        internal void Unlock() {
            if (_settings.Emulation) {
                _emulatorCache.InvokeUnlock();
                onDataMoving?.Invoke(this, (true, $"Unlock lift Machine {Id}", "emulated"));
                return;
            }

            runCommand(UnlockTheDoor(), "Unlock");
        }

        internal async Task<(DispenserStateSeverity state, VisionEsPlusResponseCodes internalState, string message)?> StatusAsync() {
            if (_settings.Emulation) {
                if (_emulatorCache.IsDoorOpened())
                    return (DispenserStateSeverity.MaintenanceService, VisionEsPlusResponseCodes.Unknown /* need to clarify*/, "the door is open");

                if (_emulatorCache.IsRebooting())
                    return (DispenserStateSeverity.Inoperable, VisionEsPlusResponseCodes.Unknown /* need to clarify*/, "inoperable");

                if (_emulatorCache.IsUnlocked())
                    return (DispenserStateSeverity.Inoperable, VisionEsPlusResponseCodes.Unknown /* need to clarify*/, "the elevator is open");

                if (_emulatorCache.IsDispensing())
                    return (DispenserStateSeverity.NeedToWait, VisionEsPlusResponseCodes.Busy /* need to clarify*/, "dispensing in progress");

                (DispenserStateSeverity, VisionEsPlusResponseCodes, string)? state = _emulatorCache.GetState();
                if (state != null)
                    return state;

                // idling
                return (DispenserStateSeverity.Normal, VisionEsPlusResponseCodes.FaultIn485Bus, "connected");
            }

            return await Task.Factory.StartNew<(DispenserStateSeverity state, VisionEsPlusResponseCodes internalState, string message)?>(() =>
                ExecuteCommand(StatusCommand(), "Status", (response, code) => {
                    switch (code) {
                        case VisionEsPlusResponseCodes.Unknown:
                            return (DispenserStateSeverity.Inoperable, code, "inoperable");
                        case VisionEsPlusResponseCodes.Ok:
                        case VisionEsPlusResponseCodes.Ready:
                        case VisionEsPlusResponseCodes.FaultIn485Bus:
                            return (DispenserStateSeverity.Normal, code, "connected");
                        default:
                            switch (TryGetDoorState(response)) {
                                case DoorState.DoorClosed:
                                    return (DispenserStateSeverity.Normal, code, "the door is closed");
                                case DoorState.DoorOpen:
                                    return (DispenserStateSeverity.MaintenanceService, code, "the door is open");
                                case DoorState.Unknown:
                                default:
                                    return (DispenserStateSeverity.Inoperable, code, "inoperable");
                            }
                    }
                }));
        }

        internal async Task<Cart> DispenseAsync(Cart cart) {
            (DispenserStateSeverity state, VisionEsPlusResponseCodes internalState, string message)? state = await StatusAsync();

            if (state?.state != DispenserStateSeverity.Normal)
                return cart;

            ChangeLight(true); // turn on lights before dispensing 
            cart = await DispenseSessionAsync(cart); // run #1
            if (cart.Items.Any())
                cart = await DispenseSessionAsync(cart); // run #2. JIC
            ChangeLight(false); // turn off lights

            return cart;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cart"></param>
        /// <param name="takeLast">0- it's the first run which tries to leave the last one product on the belt. 1- dispense even the last item of product</param>
        /// <returns></returns>
        private async Task<Cart> DispenseSessionAsync(Cart cart) {
            Func<int, BeltWithdrawalRules, List<RouteBalance>> _calculateBalances = (minTray, withdrawalRules) => {
                Pog planogram = _calcPlanogram();

                // all active and not empty routes of the current machine with the cart products
                Dictionary<string, IEnumerable<RouteBalance>> prodRoutes = planogram.Products.Where(x => cart.Products.Contains(x.Product))
                    .Select(x => new KeyValuePair<string, IEnumerable<PogRoute>>(x.Product, x.Routes.Where(r => (r.Active ?? true) && r.Quantity > (withdrawalRules != null && withdrawalRules[x.Product] ? 0 : 1))))
                    .ToDictionary(x => x.Key, x => x.Value.Select(y => new RouteBalance { Address = y.Address, Quantity = y.Quantity, Sku = planogram[y.Address].Product }));

                List<RouteBalance> balances = prodRoutes.SelectMany(x => x.Value)
                    .Where(x => ((EspBeltAddress)x.Address).Tray >= minTray) // ignore trays that are currently lower than the elevator
                    .OrderBy(x => {
                        EspBeltAddress espAddr = (EspBeltAddress)x.Address;
                        return $"{espAddr.Tray}/{x.Sku}/{x.Quantity}"; // sort routes to dispense from. Tray- from bottom to top, Belt- sort by product and remains
                    }).ToList();

                List<RouteBalance> result = new List<RouteBalance>();
                // now let's exclude inactive belts
                foreach (var b in balances)
                    if (IsBeltActive(b.Address) ?? false)
                        result.Add(b);
                return result;
            };

            Func<int, List<Slot>> _rebuildSlotChain = minTray => { // as soon as we can only ascend to get products we have to exclude lower trays
                List<RouteBalance> balances = _calculateBalances(minTray, null);

                // let's check if there's a shortage of products. If it has a place to be, than allow to dispence the last product on the belts
                IEnumerable<CartItem> shortage = cart.Items.Where(x => x.Quantity > balances.Where(y => y.Sku == x.Sku).Count());
                if (shortage.Any()) {
                    BeltWithdrawalRules rule = new BeltWithdrawalRules(shortage.Select(x => x.Sku)); // these products can be dispensed to the end
                    balances = _calculateBalances(minTray, rule);
                }

                List<Slot> result = new List<Slot>();

                foreach (var b in balances)
                    for (int i = 0; i < b.Quantity; i++)
                        result.Add(new Slot { Address = b.Address, Sku = b.Sku });

                return result;
            };

            // calculates weight of the next product to dispense
            Func<List<Slot>, int> _getNextWeight = slots => {
                if (!slots.Any() || slots.Count == 1) // if no more slots available to dispense
                    return 0;

                Slot current = slots.First();
                if ((cart[current.Sku] - 1) == 0 || slots.Where(x => x.Sku == current.Sku).Count() == 1) // we won't be trying to dispense (or can't dispense) this product, so we can easily remove similar slots
                    slots.RemoveAll(x => x.Sku == current.Sku);

                // the first slot in the 'reconsidered' list is the one we need
                return slots.Any() ? _planogram.GetProductWeight(slots[0].Sku) : 0;
            };

            (DispenserStateSeverity state, VisionEsPlusResponseCodes internalState, string message)? state;
            List<DispenseEventArgs> droppedIntoTheElevatorProducts = new List<DispenseEventArgs>();

            int addedWeight = 0; // #5122

            List<Slot> slots = _rebuildSlotChain(0);

            while (slots.Any()) {
                Slot slot = slots.First();

                int productWeight = _planogram.GetProductWeight(slot.Sku);
                int nextProductWeigth = _getNextWeight(slots);
                bool isTheVeryLastProduct = slots.Count() == 1 || (!slots.Any(x => x.Sku != slot.Sku) && cart[slot.Sku] == 1); // there's one product of this kind to dispense and no other products on the dispensing list
                addedWeight += productWeight;
                bool sendVEND = addedWeight + nextProductWeigth > _settings.MaxExtractWeightPerTime || isTheVeryLastProduct; // Send VEND command if the last product to dispense or max weight threshold reached

                onDispensing?.Invoke(this, DispenseEventArgs.DispensingStarted(slot.Address, cart.SessionId)); // notify about dispensing product started
                state = null;
                Dispense(slot.Address, sendVEND);

                if (sendVEND)
                    addedWeight = 0; // flush elevator used weight

                #region handle result
                Stopwatch sw = new Stopwatch();
                sw.Start();
                bool errorOccured = false;
                while ((state == null || state.Value.state == DispenserStateSeverity.Inoperable || state.Value.state == DispenserStateSeverity.NeedToWait)
                    && sw.Elapsed.TotalSeconds < 30) {
                    try {
                        await Task.Delay(_settings.Emulation ? 300 : 3000);
                        state = await StatusAsync(); // Wait for the next not empty state
                        if (state.HasValue) {
                            if (state.Value.internalState == VisionEsPlusResponseCodes.EmptyChannel) { // belt is empty
                                onAddressUnavailable?.Invoke(this, new DispenseFailedEventArgs { address = slot.Address, emptyBelt = true, sessionId = cart.SessionId });
                                errorOccured = true;
                                break;
                            }
                            else if (state.Value.internalState == VisionEsPlusResponseCodes.InvalidChannelRequested) { // there's no such a belt
                                onAddressUnavailable?.Invoke(this, new DispenseFailedEventArgs { address = slot.Address, emptyBelt = false, sessionId = cart.SessionId });
                                errorOccured = true;
                                break;
                            }
                        }
                    }
                    catch (SocketException) { }
                    catch (Exception ex) { }

                    switch (state?.state) {
                        case DispenserStateSeverity.Normal: // the product was extracted from the belt to the elevator successfully
                            var de = DispenseEventArgs.DispensingFinished(slot.Address, cart.SessionId);
                            droppedIntoTheElevatorProducts.Add(de);
                            onDispensed?.Invoke(this, de);
                            cart.RemoveDispensed(slot.Sku); // remove dispensed item
                            slots = _rebuildSlotChain(((EspBeltAddress)slot.Address).Tray); // product dispensed. Let's rebuild the chain

                            state = null;
                            state = await StatusAsync(); // Check if the machine ready to handle next command

                            if (state?.state != DispenserStateSeverity.Normal) // The machine is in error state
                            {
                                switch (state?.internalState) {
                                    case VisionEsPlusResponseCodes.WaitingForProductToBeRemoved:
                                        addedWeight = 0; // let's drop the value JIC. Because the machine can park the elevator without VEND command. For example, the sensor was blocked by a volumetric product 

                                        onWaitingProductsToBeRemoved?.Invoke(this, droppedIntoTheElevatorProducts);
                                        state = null;

                                        Stopwatch sw1 = new Stopwatch();
                                        sw1.Start();

                                        while ((state == null ||
                                            state.Value.state == DispenserStateSeverity.Inoperable ||
                                            state.Value.internalState == VisionEsPlusResponseCodes.WaitingForProductToBeRemoved) && sw1.Elapsed.TotalSeconds < 120) {
                                            try {
                                                Thread.Sleep(3000);
                                                state = await StatusAsync(); // Wait for the next not empty state 
                                            }
                                            catch (SocketException) { }
                                            catch (Exception ex) { }
                                        }
                                        sw1.Stop();

                                        if (state?.internalState == VisionEsPlusResponseCodes.Ready || state?.internalState == VisionEsPlusResponseCodes.FaultIn485Bus) // The product/s was/were given
                                            droppedIntoTheElevatorProducts.Clear(); // Consider the products as dispensed
                                        else if (state?.internalState == VisionEsPlusResponseCodes.WaitingForProductToBeRemoved) {
                                            // Looks like the customer has forgotten to pick up the products. At least they're in the elevator so far
                                            foreach (var p in droppedIntoTheElevatorProducts)
                                                onAbandonment?.Invoke(this, p);

                                            droppedIntoTheElevatorProducts.Clear(); // Consider the products as disputable, but as for now forget them
                                        }
                                        else if (state?.internalState == VisionEsPlusResponseCodes.FaultInProductDetector) {
                                            // This could happen when during the descent the product fell and stopped blocking the detector or, conversely, turned over and blocked it.
                                            // This doesn't mean that the product wasn't issued
                                        }

                                        break;
                                    default:
                                        break;
                                }
                            }

                            break;
                        case DispenserStateSeverity.NeedToWait:
                            //if (state.Value.message)
                            // Log(...);
                            break;
                        case DispenserStateSeverity.MaintenanceService:
                            errorOccured = true;
                            onFailedToDispense?.Invoke(this, new FailedToDispenseEventArgs { sessionId = cart.SessionId, ProductsNotGivenFromAddresses = cart.Items.ToDictionary(x => x.Sku, x => x.Quantity) });
                            break;
                        case DispenserStateSeverity.Inoperable:
                            // todo
                            break;
                        default:
                            break;
                    }
                }
                sw.Stop();

                if (errorOccured) {
                    slots = _rebuildSlotChain(((EspBeltAddress)slot.Address).Tray); // product wasn't dispensed. Let's rebuild the chain
                    continue;
                }
                #endregion
            }

            return cart;
        }

        private void Dispense(EspBeltAddress address, bool lastCommand) {
            if (_settings.Emulation) {
                _emulatorCache.InvokeDispense();

                Pog p = _calcPlanogram();
                PogRoute route = p.GetRoute(address);
                if (route.MockedQuantity < 1)
                    _emulatorCache.RaiseEmptyBelt(address);
                else if (route.MockedActive.HasValue && !route.MockedActive.Value)
                    _emulatorCache.RaiseInvalidAddress(address);

                onDataMoving?.Invoke(this, (true, $"Dispense from {address}", "emulated"));
                return;
            }

            runCommand(DispenseCommand(address.Tray, address.Belt, lastCommand), "Dispense");
        }

        internal bool? IsBeltActive(string route) {
            EspBeltAddress address = route;

            if (address.Machine != Id) // The route belongs to another machine
                return null;

            if (_settings.Emulation) {
                Pog p = _calcPlanogram();
                PogRoute r = p.GetRoute(route);
                bool? isActive = r.MockedActive ?? r.Active;
                onDataMoving?.Invoke(this, (true, $"Check belt {route}", "emulated"));
                return isActive;
            }

            return ExecuteCommand(CheckChannelCommand(address.Tray, address.Belt), "CheckBelt", (response, code) =>
                response.Length == 8 && (response[4] == 0x43 || response[4] == 0x41)); // 0x44 means bealt is unavailable
        }

        internal async Task<bool> ActivateBeltAsync(string route) {
            EspBeltAddress address = route;

            if (address.Machine != Id) // The route belongs to another machine
                return false;

            return ExecuteCommand(ActivateBeltCommand(address.Tray, address.Belt), "ActivateBelt", (response, code) =>
                response.Length == 8 && response[4] == 0x43); // 0x44 means bealt is unavailable}
        }
        #endregion

        #region Commands
        /// <summary>
        /// Turn on/off the light
        /// </summary>
        /// <returns>команда</returns>
        private byte[] ChangeLightCommand(bool isOn) {
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
        private byte[] StatusCommand() {
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
        private byte[] CheckChannelCommand(int tray, int belt) {
            var comm = new byte[_commandBody.Length];
            Array.Copy(_commandBody, comm, _commandBody.Length);
            comm[4] = 0x43;
            comm[5] = 0x43;
            comm[6] = (byte)(tray * 10 + belt);
            InjectCheckSumm(comm);
            return comm;
        }

        private byte[] ActivateBeltCommand(int tray, int belt) {
            var comm = new byte[_commandBody.Length];
            Array.Copy(_commandBody, comm, _commandBody.Length);
            comm[4] = 0x52;
            comm[5] = 0x80;
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
        private byte[] DispenseCommand(int tray, int belt, bool lastItem) {
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
        private byte[] ParkingCommand() {
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
        private byte[] ResetCommand() {
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
        private byte[] UnlockTheDoor() {
            var retval = new byte[_commandBody.Length];
            Array.Copy(_commandBody, retval, _commandBody.Length);
            retval[4] = 0x4E;
            InjectCheckSumm(retval);
            return retval;
        }
        #endregion

        #region Private
        private DoorState TryGetDoorState(byte[] arr) {
            if (arr == null || arr.Length != 7 || arr[2] != 0x50)
                return DoorState.Unknown;

            var doorState = (DoorState)arr[3];
            return doorState;
        }

        /// <summary>
        /// Вставляет во второй и третий с конца байты контрольную сумму в соответствии с протоколом
        /// </summary>
        /// <param name="message">сообщение</param>
        private void InjectCheckSumm(IList<byte> message) {
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

        private void runCommand(byte[] command, string message, Action<VisionEsPlusResponseCodes> postAction = null) {
            lock (_channel) {
                byte[] response = null;

                try {
                    onDataMoving?.Invoke(this, (true, $"{message} Machine {Id}", _bitConvert(command))); // Send telemetry to subscribers via DispenserNegotiatorLogger
                    response = _channel.SendCommand(command); // Send command to the device
                }
                catch (SocketException) { }

                VisionEsPlusResponseCodes code = ParseResponse(response);

                postAction?.Invoke(code);

                onDataMoving?.Invoke(this, (false, $"{message} Machine {Id}: {code}", _bitConvert(response)));
            }
        }

        private T ExecuteCommand<T>(byte[] command, string message, Func<byte[], VisionEsPlusResponseCodes, T> postAction) {
            lock (_channel) {
                byte[] response = null;

                try {
                    onDataMoving?.Invoke(this, (true, $"{message} Machine {Id}", _bitConvert(command))); // Send telemetry to subscribers via DispenserNegotialorLogger
                    response = _channel.SendCommand(command); // Send command to the device
                }
                catch (SocketException) { }

                VisionEsPlusResponseCodes code = ParseResponse(response);

                onDataMoving?.Invoke(this, (false, $"{message} Machine {Id}: {code}", _bitConvert(response)));

                return postAction.Invoke(response, code);
            }
        }

        private string _bitConvert(byte[] data) => BitConverter.ToString(data).Replace('-', ' ');

        /// <summary>
        /// Fetch current planogram
        /// </summary>
        /// <returns></returns>
        private Pog _calcPlanogram() {
            Pog fullPlanogram = _getPlanogram?.Invoke();
            return fullPlanogram.GetPartialPlanogram(x => ((EspBeltAddress)x).Machine == Id);
        }
        #endregion

        private readonly Func<Pog> _getPlanogram;
        private readonly Pog _planogram;
        private readonly VisionEsPlusSettings _settings;
        private readonly byte[] _commandBody;
        private ICommunicationChannel _channel;
        private VisionEsPlusEmulationCache _emulatorCache;
    }
}