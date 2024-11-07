using Filuet.Hardware.Dispensers.Abstractions;
using Filuet.Hardware.Dispensers.Abstractions.Models;
using Filuet.Hardware.Dispensers.Core.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

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
            DispensingChainBuilder chainBuilder,
            PoG planogram) {
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
                    PingRoutes();
                };
                d.onWaitingProductsToBeRemoved += (sender, e) => onWaitingProductsToBeRemoved?.Invoke(sender, e);
                d.onFailedToDispense += (sender, e) => Dispense(false, e.ProductsNotGivenFromAddresses.Select(x => (_planogram.GetProduct(x.Key).ProductUid, (ushort)x.Value)).ToArray());
                d.onAddressUnavailable += (sender, e) => {
                    _planogram.SetAttributes(e.address, d, false);
                    if (e.emptyBelt)
                        _planogram.GetRoute(e.address).Quantity = 0;

                    onPlanogramClarification?.Invoke(this, new PlanogramEventArgs { Planogram = _planogram });
                };
            }

            foreach (ILightEmitter l in _lightEmitters)
                l.onLightsChanged += (sender, e) => onLightsChanged?.Invoke(this, e);

            _chainBuilder = chainBuilder;
            _planogram = planogram;

            TestAsync().ConfigureAwait(false);
        }

        public async Task DispenseAsync(Cart cart) {
            try {
                await TestAsync();
                // leave only active dispensers
                IEnumerable<IDispenser> dispensers = _dispensers.Where(x => x.IsAvailable);

                // sort dispensers from most loaded to the less loaded taking into account products from the cart
                List<uint> dispensersOrder = _planogram.Products.Where(x => cart.Products.Contains(x.ProductUid)).SelectMany(x => x.Routes)
                    .Where(x => dispensers.Any(d => d.Id == x.Dispenser.Id))
                    .GroupBy(x => x.Dispenser.Id).Select(group => new { group.Key, Qty = group.ToList().Sum(x => x.Quantity) })
                    .OrderByDescending(x => x.Qty).Select(x => x.Key).ToList();
                
                foreach (var dispenserId in dispensersOrder) {
                    IDispenser dispenser = dispensers.First(x => x.Id == dispenserId);
                    IEnumerable<CartItem> items = await dispenser.DispenseAsync(cart);
                    cart.RemoveDispensed(items); // remove dispensed and go to the next dispenser
                }
            }
            catch (InvalidOperationException ex) {
                onFailed?.Invoke(this, new DispenseFailEventArgs { message = ex.Message });
            }
        }

        public void Unlock(params uint[] machines) {
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

        private async Task PingRoutes()
            => await Task.WhenAll(_dispensers.Select(x => x.TestAsync()).ToArray())
                .ContinueWith(x => {
                    Parallel.ForEach(_dispensers, d => {
                        var pingResult = d.Ping(_planogram.Addresses.ToArray()).ToList();
                        foreach (var a in pingResult)
                            if (a.isActive.HasValue)
                                _planogram.SetAttributes(a.address, d, a.isActive.Value);
                    });

                    onPlanogramClarification?.Invoke(this, new PlanogramEventArgs { Planogram = _planogram });
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
                        _planogram.SetAttributes(a.Item1, d, isActive);
                    }
            }

            onPlanogramClarification?.Invoke(this, new PlanogramEventArgs { Planogram = _planogram });

            return isActive;
        }

        internal readonly IEnumerable<IDispenser> _dispensers;
        internal readonly IEnumerable<ILightEmitter> _lightEmitters;
        private readonly DispensingChainBuilder _chainBuilder;
        internal readonly PoG _planogram;
    }
}