using System;

namespace Filuet.Hardware.Dispensers.Abstractions
{
    public class ResetEventArgs : EventArgs
    {
        public int MachineId { get; set; }
    }
}
