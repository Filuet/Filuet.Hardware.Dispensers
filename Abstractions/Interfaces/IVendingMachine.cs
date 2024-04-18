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
        event EventHandler<(bool direction, string message, string data)> onDataMoving;
        event EventHandler<DispenseEventArgs> onDispensing;
        event EventHandler<DispenseEventArgs> onDispensed;
        event EventHandler<DispenseEventArgs> onAbandonment;
        event EventHandler<VendingMachineTestEventArgs> onTest;
        event EventHandler<DispenseFailEventArgs> onFailed;
        event EventHandler<PlanogramEventArgs> onPlanogramClarification;
        event EventHandler<LightEmitterEventArgs> onLightsChanged;
        event EventHandler<UnlockEventArgs> onMachineUnlocked;

        Task Dispense(params (string productUid, ushort quantity)[] items);
        void Unlock(params uint[] machines);
        Task Test();
    }
}