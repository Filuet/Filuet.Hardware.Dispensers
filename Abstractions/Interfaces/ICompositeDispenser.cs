using System;

namespace Filuet.Hardware.Dispensers.Abstractions
{
    /// <summary>
    /// Composite product dispenser
    /// </summary>
    /// <remarks>Can consist of any quantity of single dispensers</remarks>
    public interface ICompositeDispenser
    {
        event EventHandler<DispenseEventArgs> OnDispensing;

        event EventHandler<ProductDispensedEventArgs> OnDispensingFinished;

        event EventHandler<DispenserTestEventArgs> OnTest;

        void Dispense(params (string productUid, ushort quantity)[] items);

        void CheckChannel(string route);
    }
}