using System;
using System.Threading.Tasks;

namespace Filuet.Hardware.Dispensers.Abstractions
{
    /// <summary>
    /// Vending machine interface
    /// </summary>
    /// <remarks>Can consist of any quantity of single dispensers</remarks>
    public interface IVendingMachine
    {
        event EventHandler<string> onResponse;

        event EventHandler<DispenseEventArgs> onDispensed;

        event EventHandler<ProductDispensedEventArgs> onDispensingFinished;

        event EventHandler<VendingMachineTestEventArgs> onTest;

        event EventHandler<DispenseFailEventArgs> onFailed;

        event EventHandler<PlanogramEventArgs> onPlanogramClarification;

        event EventHandler<LightEmitterEventArgs> onLightsChanged;

        Task Dispense(params (string productUid, ushort quantity)[] items);

        void Unlock(params uint[] machines);

        Task Test();
    }
}