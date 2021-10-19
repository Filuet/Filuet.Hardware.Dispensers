using Filuet.ASC.Kiosk.OnBoard.Common.Abstractions.Hardware;
using Filuet.ASC.Kiosk.OnBoard.Dispensing.Abstractions.Models;
using Filuet.ASC.Kiosk.OnBoard.Ordering.Abstractions;
using System;

namespace Filuet.ASC.Kiosk.OnBoard.Dispensing.Abstractions
{
    /// <summary>
    /// Composite product dispenser
    /// </summary>
    /// <remarks>Can consist of any quantity of single dispensers</remarks>
    public interface ICompositeDispenser
    {
        event EventHandler<DispenseEventArgs> OnDispensing;

        event EventHandler<ProductDispensedEventArgs> OnDispensingFinished;

        event EventHandler<DeviceTestEventArgs> OnTest;

        void Dispense(params OrderItem[] items);

        void CheckChannel(CompositDispenseAddress address);
    }
}