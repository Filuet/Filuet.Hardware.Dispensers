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
        public event EventHandler<DispenseEventArgs> onWaitingProductsToBeRemoved;

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
            }

            foreach (ILightEmitter l in _lightEmitters)
                l.onLightsChanged += (sender, e) => onLightsChanged?.Invoke(this, e);

            _chainBuilder = chainBuilder;
            _planogram = planogram;

            TestAsync().ConfigureAwait(false);
        }

        public async Task Dispense(params (string productUid, ushort quantity)[] items) {
            try {
                await TestAsync();

                IEnumerable<DispenseCommand> dispensingChain = _chainBuilder.BuildChain(items, address => {
                    PoGRoute route = _planogram.GetRoute(address);
                    return route.Dispenser.GetAddressRank(address);
                }, x => PingRoute(x));

                IEnumerable<IDispenser> dispensers = dispensingChain.Select(x => x.Route.Dispenser).Distinct().OrderBy(x => x.Id);

                foreach (var dispenser in dispensers) {
                    await dispenser.DispenseAsync(dispensingChain.Where(x => x.Route.Dispenser == dispenser).ToDictionary(x => x.Route.Address, y => (uint)y.Quantity));
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