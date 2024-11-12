using Filuet.Hardware.Dispensers.Abstractions;
using Filuet.Hardware.Dispensers.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml.Linq;

[assembly: InternalsVisibleTo("PoC")]

namespace Filuet.Hardware.Dispensers.Core
{
    internal class VendingMachine : IVendingMachine
    {
        public event EventHandler<(bool direction, string message, string data)> onDataMoving;
        public event EventHandler<DispenseEventArgs> onDispensing;
        public event EventHandler<DispenseEventArgs> onDispensed;
        public event EventHandler<DispenseEventArgs> onAbandonment;
        public event EventHandler<VendingMachineTestEventArgs> onTest;
        public event EventHandler<DispenseFailEventArgs> onFailed;
        public event EventHandler<PlanogramEventArgs> onPlanogramClarification;
        public event EventHandler<LightEmitterEventArgs> onLightsChanged;
        public event EventHandler<UnlockEventArgs> onMachineUnlocked;
        public event EventHandler<IEnumerable<DispenseEventArgs>> onWaitingProductsToBeRemoved;

        public VendingMachine(IEnumerable<IDispenser> dispensers,
            IEnumerable<ILightEmitter> lightEmitters,
            Pog planogram) {
            _dispensers = dispensers;
            _lightEmitters = lightEmitters;

            foreach (IDispenser d in _dispensers) {
                d.onTest += (sender, e) => onTest?.Invoke(this, new VendingMachineTestEventArgs { Dispenser = d, Severity = e.Severity, Message = e.Message });
                d.onDataMoving += (sender, e) => onDataMoving?.Invoke(sender, e);
                d.onDispensing += (sender, e) => onDispensing?.Invoke(sender, e);
                d.onDispensed += (sender, e) => onDispensed?.Invoke(sender, e);
                d.onAbandonment += (sender, e) => onAbandonment?.Invoke(sender, e);
                d.onReset += (sender, e) => {
                    // Check routes right after reset. It can be that some routes have been enabled/disabled recently
                    PingRoutesAsync(e.MachineId).RunSynchronously();
                };
                d.onWaitingProductsToBeRemoved += (sender, e) => {
                    foreach (var a in e) {
                        PogRoute r = _planogram.GetRoute(a.address);
                        r.Quantity--;
                        if (r.MockedQuantity.HasValue) // emulation
                            r.MockedQuantity--;
                    }

                    onWaitingProductsToBeRemoved?.Invoke(sender, e);
                    onPlanogramClarification?.Invoke(this, new PlanogramEventArgs { Planogram = _planogram, Comment = comment, MachineId = d.Id });
                };
                d.onAddressUnavailable += (sender, e) => {
                    PogRoute route = _planogram.GetRoute(e.address);
                    bool? formerValue = route.Active;
                    int formerQty = route.Quantity;

                    string comment = null;

                    _planogram.SetAttributes(e.address, false);
                    if (e.emptyBelt) {
                        route.Quantity = 0;
                        if (route.MockedQuantity.HasValue) // emulation
                            route.MockedQuantity = 0;

                        if (formerQty != 0)
                            comment = $"Quantity changed from {formerQty} to 0";
                    }
                    else {
                        route.Active = false;
                        if (route.MockedActive.HasValue) // emulation
                            route.MockedActive = false;

                        if (!formerValue.HasValue || formerValue.Value)
                            comment = "The route deactivated";
                    }

                    onPlanogramClarification?.Invoke(this, new PlanogramEventArgs { Planogram = _planogram, Comment = comment, MachineId = d.Id });
                };
            }

            foreach (ILightEmitter l in _lightEmitters)
                l.onLightsChanged += (sender, e) => onLightsChanged?.Invoke(this, e);

            _planogram = planogram;

            Task.Delay(1000).ContinueWith(t => TestAsync().ConfigureAwait(false));
            PingRoutesAsync(_dispensers.Select(x => x.Id).ToArray()).ConfigureAwait(false);
        }

        public async Task DispenseAsync(Cart cart) {
            try {
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

                foreach (var x in dispenserRank.OrderByDescending(x=>x.qty)) {
                    cart = await x.dispenser.DispenseAsync(cart);
                }
            }
            catch (InvalidOperationException ex) {
                onFailed?.Invoke(this, new DispenseFailEventArgs { message = ex.Message });
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

                    onPlanogramClarification?.Invoke(this, new PlanogramEventArgs { Planogram = _planogram, Comment = "Routes checked" });
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

            onPlanogramClarification?.Invoke(this, new PlanogramEventArgs { Planogram = _planogram });

            return isActive;
        }

        internal readonly IEnumerable<IDispenser> _dispensers;
        internal readonly IEnumerable<ILightEmitter> _lightEmitters;
        internal readonly Pog _planogram;
    }
}