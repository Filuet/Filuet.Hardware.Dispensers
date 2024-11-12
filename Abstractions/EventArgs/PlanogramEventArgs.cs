using Filuet.Hardware.Dispensers.Abstractions.Models;
using System;

namespace Filuet.Hardware.Dispensers.Abstractions
{
    public class PlanogramEventArgs : EventArgs
    {
        public Pog Planogram { get; set; }

        public int MachineId { get; set; }

        public string Comment { get; set; }
    }
}