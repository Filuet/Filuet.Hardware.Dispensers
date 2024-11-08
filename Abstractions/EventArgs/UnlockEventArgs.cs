using System;

namespace Filuet.Hardware.Dispensers.Abstractions
{
    public class UnlockEventArgs : EventArgs
    {
        public int machine { get; set; }

        public static UnlockEventArgs Create(int machine)
            => new UnlockEventArgs { machine = machine };
    }
}