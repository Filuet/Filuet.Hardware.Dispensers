using Filuet.Hardware.Dispensers.Abstractions.Interfaces;
using System;

namespace Filuet.Hardware.Dispensers.Abstractions.Models
{
    public abstract class Belt : LayoutUnit, IBelt
    {
        public Tray Tray { get; private set; }

        public virtual DispensingRoute Address { get; }

        internal void SetTray(Tray tray)
        {
            if (tray == null)
                throw new ArgumentException("Tray must be specified");

            Tray = tray;
        }

        public override string ToString() => $"Belt № {Number}";
    }
}