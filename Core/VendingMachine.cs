using Filuet.Hardware.Dispensers.Abstractions;
using Filuet.Hardware.Dispensers.Abstractions.Helpers;
using Filuet.Hardware.Dispensers.Abstractions.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("PoC")]

namespace Filuet.Hardware.Dispensers.Core
{
    public class VendingMachine : IVendingMachine
    {
        public event EventHandler<(bool direction, string message, string data)> onDataMoving;
        public event EventHandler<AddressEventArgs> onAbandonment;
        public event EventHandler<AddressEventArgs> onAddressInactive;
        public event EventHandler<AddressEventArgs> onDispensing;
        public event EventHandler<AddressEventArgs> onDispensed;
        public event EventHandler<VendingMachineTestEventArgs> onTest;
        public event EventHandler<DispensingFailedEventArgs> onFailed;
        public event EventHandler<PlanogramEventArgs> onPlanogramClarification;
        public event EventHandler<LightEmitterEventArgs> onLightsChanged;
        public event EventHandler<UnlockEventArgs> onMachineUnlocked;
        public event EventHandler<VendingMachineTestEventArgs> onDispensedFromUnit;
        public event EventHandler<IEnumerable<AddressEventArgs>> onWaitingProductsToBeRemoved;
        public event EventHandler<EventArgs> onDispensingFinished;

        internal VendingMachine(IEnumerable<IDispenser> dispensers,
            IEnumerable<ILightEmitter> lightEmitters,
            Pog planogram,
            ILogger<VendingMachine> logger) {
            _dispensers = dispensers;
            _lightEmitters = lightEmitters;
            _logger = logger;

            foreach (IDispenser d in _dispensers) {
                d.onTest += (sender, e) => {
                    onTest?.Invoke(this, new VendingMachineTestEventArgs { Dispenser = d, Severity = e.Severity, Message = e.Message });
                    _logger.Log(e.Severity.ToLogLevel(), $"Dispenser {d.Id}. {e.Message}");
                };

                d.onDataMoving += (sender, e) => {
                    onDataMoving?.Invoke(sender, e);
                    _logger.LogInformation($"{(e.direction ? "→" : "←")} {e.data} [{e.message}]");
                };

                d.onDispensing += (sender, e) => {
                    onDispensing?.Invoke(sender, e);
                    _logger.LogInformation($"{e.sessionId} Dispensing from {e.address} started");
                };

                d.onDispensed += (sender, e) => {
                    PogRoute r = _planogram.GetRoute(e.address);
                    r.Quantity--;
                    if (r.MockedQuantity.HasValue) // emulation
                        r.MockedQuantity--;

                    string planogramComment = $"dispensed from {r.Address}";
                    onPlanogramClarification?.Invoke(this, new PlanogramEventArgs { planogram = _planogram, comment = planogramComment, machineId = d.Id, sessionId = e.sessionId });
                    _logger.LogError(planogramComment);

                    onDispensed?.Invoke(sender, e);
                    _logger.LogInformation($"{e.sessionId} {e.address} dispensed");
                };

                d.onAbandonment += (sender, e) => {
                    onAbandonment?.Invoke(sender, e);
                    _logger.LogWarning($"{e.sessionId} {e.address} wasn't taken");
                };

                d.onAddressInactive += (sender, e) => {
                    PogRoute route = _planogram.GetRoute(e.address);
                    _planogram.SetAttributes(e.address, false);
                    if (route.MockedActive.HasValue) // emulation
                        route.MockedActive = false;

                    _logger.LogWarning($"{e.sessionId} {e.address} inactive and disabled");
                    onAddressInactive?.Invoke(this, new AddressEventArgs { message = "Address's inactive and disabled", address = e.address, sessionId = e.sessionId });

                    string planogramComment = $"{e.address} is inactive and disabled";
                    onPlanogramClarification?.Invoke(this, new PlanogramEventArgs { planogram = _planogram, comment = planogramComment, machineId = d.Id, sessionId = e.sessionId });
                    _logger.LogError(planogramComment);
                };
                
                d.onReset += (sender, e) => {
                    _logger.LogInformation($"{e.MachineId} reset invoked");
                    // Check routes right after reset. It can be that some routes have been enabled/disabled recently
                    PingRoutesAsync(e.MachineId).RunSynchronously();
                };

                d.onWaitingProductsToBeRemoved += (sender, e) => {
                    onWaitingProductsToBeRemoved?.Invoke(sender, e);

                    foreach (var i in e)
                        _logger.LogInformation($"{i.sessionId} An item from {i.address} ready to be taken");
                };
                d.onFailedToDispense += (sender, e) =>
                {
                    _logger.LogInformation($"{e.sessionId} skus: {string.Join(',', e.ProductsNotGivenFromAddresses.Keys)}, with quantity/s: {string.Join(',', e.ProductsNotGivenFromAddresses.Values)} could have been failed!");
                };

                d.onAddressUnavailable += (sender, e) => {
                    PogRoute route = _planogram.GetRoute(e.address);
                    bool? formerValue = route.Active;
                    int formerQty = route.Quantity;

                    string planogramComment = null;
                    string errorMessage = null;

                    _planogram.SetAttributes(e.address, false);
                    if (e.emptyBelt) {
                        route.Quantity = 0;
                        if (route.MockedQuantity.HasValue) // emulation
                            route.MockedQuantity = 0;

                        if (formerQty != 0) {
                            planogramComment = $"{e.address} is empty and blocked. Qty changed: {formerQty}→0";
                            errorMessage = "Empty belt address found";
                        }
                    }
                    else {
                        if (route.MockedActive.HasValue) // emulation
                            route.MockedActive = false;

                        if (!formerValue.HasValue || formerValue.Value) {
                            planogramComment = $"{e.address} is inactive and disabled";
                            errorMessage = "address is inactive and disabled";
                        }
                    }

                    _logger.LogError($"{e.sessionId} {e.address} {errorMessage}");
                    onFailed?.Invoke(this, new DispensingFailedEventArgs { address = e.address, emptyBelt = e.emptyBelt, message = errorMessage, sessionId = e.sessionId, Sku = e.Sku });
                    onPlanogramClarification?.Invoke(this, new PlanogramEventArgs { planogram = _planogram, comment = planogramComment, machineId = d.Id, sessionId = e.sessionId });
                    _logger.LogError(planogramComment);
                };
            }

            foreach (ILightEmitter l in _lightEmitters)
                l.onLightsChanged += (sender, e) => {
                    onLightsChanged?.Invoke(this, e);
                    _logger.LogInformation($"Light emitter {e.Id}: {(e.IsOn ? "on" : "off")}");
                };

            _planogram = planogram;

            Task.Delay(1000).ContinueWith(t => TestAsync().ConfigureAwait(false));
            // Let's spare some time and don't ping routes
            // PingRoutesAsync(_dispensers.Select(x => x.Id).ToArray()).ConfigureAwait(false);
        }
        public void UpdatePlanogram(Pog newPlanogram)
        {
            if (newPlanogram == null)
            {
                _logger.LogWarning("Attempted to update VendingMachine with a null planogram.");
                return;
            }

            _planogram = newPlanogram;
            _logger.LogInformation("VendingMachine planogram updated successfully.");
        }

        public async Task DispenseAsync(Cart cart) {
            try {
                cart.SessionId = DateTime.Now.ToString("HHmm");

                await TestAsync();
                // leave only active dispensers
                IEnumerable<IDispenser> dispensers = _dispensers.Where(x => x.IsAvailable);

                List<(IDispenser dispenser, int qty)> dispenserRank = new List<(IDispenser, int)>();

                foreach (var d in dispensers) {
                    IEnumerable<PogRoute> candidates = _planogram.Products.Where(x => cart.Products.Contains(x.Product)).SelectMany(x => x.Routes).Where(x => d.Id == x.DispenserId);
                    int qty = candidates.Sum(x => x.Quantity);
                    if (qty > 0)
                        dispenserRank.Add((d, qty));
                }

                if (!dispenserRank.Any()) {
                    string errorMessage = $"No products found to dispense. Dispenser might be inoperable";
                    onFailed?.Invoke(this, new DispensingFailedEventArgs { message = errorMessage, Sku = string.Join(",", cart.Products)});
                    _logger.LogError(errorMessage);
                }

                foreach (var x in dispenserRank.OrderByDescending(x => x.qty)) {
                    cart = await x.dispenser.DispenseAsync(cart);
                    onDispensedFromUnit?.Invoke(this, new VendingMachineTestEventArgs { Dispenser = x.dispenser, Message = "Ready to go", Severity = Abstractions.Enums.DispenserStateSeverity.Normal });
                }
            }
            catch (InvalidOperationException ex) {
                onFailed?.Invoke(this, new DispensingFailedEventArgs { message = ex.Message });
            }
            finally {
                onDispensingFinished?.Invoke(this, null);
            }
        }

        public void Unlock(params int[] machines) {
            Parallel.ForEach(_dispensers, x => {
                if (!machines.Any() || machines.Contains(x.Id)) {
                    x.Unlock();
                    onMachineUnlocked?.Invoke(this, new UnlockEventArgs { machine = x.Id });
                }
            });
        }

        /// <summary>
        /// Update status of dispensers
        /// </summary>
        /// <returns></returns>
        public async Task TestAsync()
            => await Task.WhenAll(_dispensers.Select(x => x.TestAsync()).ToArray());

        private async Task PingRoutesAsync(params int[] machines)
            => await Task.WhenAll(_dispensers.Where(x => machines == null || machines.Contains(x.Id)).Select(x => x.TestAsync()).ToArray())
                .ContinueWith(x => {
                    Parallel.ForEach(_dispensers, d => {
                        var pingResult = d.Ping(_planogram.Addresses.ToArray()).ToList();
                        foreach (var a in pingResult)
                            if (a.isActive.HasValue)
                                _planogram.SetAttributes(a.address, a.isActive.Value);
                    });

                    onPlanogramClarification?.Invoke(this, new PlanogramEventArgs { planogram = _planogram, comment = "Routes checked" });
                    _logger.LogInformation("Routes checked");
                });

        private bool PingRoute(string route) {
            bool isActive = false;

            foreach (var d in _dispensers) {
                if (!d.IsAvailable)
                    continue;

                var pingResult = d.Ping(route);
                foreach (var a in pingResult)
                    if (a.isActive.HasValue) {
                        isActive = a.isActive.Value;
                        _planogram.SetAttributes(a.Item1, isActive);
                    }
            }

            onPlanogramClarification?.Invoke(this, new PlanogramEventArgs { planogram = _planogram });

            return isActive;
        }

        internal readonly IEnumerable<IDispenser> _dispensers;
        internal readonly IEnumerable<ILightEmitter> _lightEmitters;
        internal Pog _planogram;
        private readonly ILogger<VendingMachine> _logger;
    }
}