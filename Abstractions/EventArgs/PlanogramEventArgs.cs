using Filuet.Hardware.Dispensers.Abstractions.Models;
using System;

namespace Filuet.Hardware.Dispensers.Abstractions
{
    public class PlanogramEventArgs : EventArgs
    {
        public PoG Planogram { get; set; }
    }
}