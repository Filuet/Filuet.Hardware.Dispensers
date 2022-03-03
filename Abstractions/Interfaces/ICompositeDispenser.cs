﻿using System;
using System.Threading.Tasks;

namespace Filuet.Hardware.Dispensers.Abstractions
{
    /// <summary>
    /// Composite product dispenser
    /// </summary>
    /// <remarks>Can consist of any quantity of single dispensers</remarks>
    public interface ICompositeDispenser
    {
        event EventHandler<string> onResponse;

        event EventHandler<DispenseEventArgs> onDispensed;

        event EventHandler<ProductDispensedEventArgs> onDispensingFinished;

        event EventHandler<CompositeDispenserTestEventArgs> onTest;

        event EventHandler<PlanogramEventArgs> onPlanogramClarification;

        Task Dispense(params (string productUid, ushort quantity)[] items);

        Task Test();
    }
}