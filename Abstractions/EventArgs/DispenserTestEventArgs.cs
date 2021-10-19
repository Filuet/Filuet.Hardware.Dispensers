using Filuet.Hardware.Dispensers.Abstractions.Enums;
using System;

namespace Filuet.Hardware.Dispensers.Abstractions
{
    public class DispenserTestEventArgs : EventArgs
    {
        public DispenserStateSeverity Severity { get; set; } = DispenserStateSeverity.Normal;
        public string Message { get; set; }
    }
}