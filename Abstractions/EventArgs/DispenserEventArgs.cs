using System;

namespace Filuet.Hardware.Dispensers.Abstractions
{
    public class DispenserEventArgs : EventArgs
    {
        public IDispenser dispenser { get; set; }
    }
}