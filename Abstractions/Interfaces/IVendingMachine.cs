using Filuet.Hardware.Dispensers.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Filuet.Hardware.Dispensers.Abstractions
{
    /// <summary>
    /// Vending machine interface
    /// </summary>
    /// <remarks>Can consist of any quantity of single dispensers</remarks>
    public interface IVendingMachine
    {
        event EventHandler<(bool direction, string message, string data)> onDataMoving;
        event EventHandler<AddressEventArgs> onAbandonment;
        event EventHandler<AddressEventArgs> onAddressInactive;
        event EventHandler<AddressEventArgs> onDispensing;
        event EventHandler<AddressEventArgs> onDispensed;
        event EventHandler<VendingMachineTestEventArgs> onTest;
        event EventHandler<DispensingFailedEventArgs> onFailed;
        event EventHandler<PlanogramEventArgs> onPlanogramClarification;
        event EventHandler<LightEmitterEventArgs> onLightsChanged;
        event EventHandler<UnlockEventArgs> onMachineUnlocked;
        event EventHandler<VendingMachineTestEventArgs> onDispensedFromUnit;
        event EventHandler<EventArgs> onDispensingFinished;
        event EventHandler<IEnumerable<AddressEventArgs>> onWaitingProductsToBeRemoved;
        void UpdatePlanogram(Pog newPlanogram);
        Task DispenseAsync(Cart cart);
        void Unlock(params int[] machines);
        Task TestAsync();
    }
}