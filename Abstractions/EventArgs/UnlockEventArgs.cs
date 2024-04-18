using System;

namespace Filuet.Hardware.Dispensers.Abstractions
{
    public class UnlockEventArgs : EventArgs
    {
        public uint machine { get; set; }

        public static UnlockEventArgs Create(uint machine)
            => new UnlockEventArgs { machine = machine };
    }
}