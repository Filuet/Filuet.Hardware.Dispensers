﻿using Filuet.ASC.Kiosk.OnBoard.Dispensing.Abstractions.Interfaces;
using System;

namespace Filuet.ASC.Kiosk.OnBoard.Dispensing.Abstractions.Models
{
    public abstract class Belt : LayoutUnit, IBelt
    {
        public Tray Tray { get; private set; }

        public abstract string Address { get; }

        internal void SetTray(Tray tray)
        {
            if (tray == null)
                throw new ArgumentException("Tray must be specified");

            Tray = tray;
        }

        public override string ToString() => $"Belt № {Number}";
    }
}