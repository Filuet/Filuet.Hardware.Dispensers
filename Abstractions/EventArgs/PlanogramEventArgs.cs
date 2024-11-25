using Filuet.Hardware.Dispensers.Abstractions.Models;
using System;
using System.Net;

namespace Filuet.Hardware.Dispensers.Abstractions
{
    public class PlanogramEventArgs : DispenseSessionEventArgs
    {
        public Pog planogram { get; set; }

        public int machineId { get; set; }

        public string comment { get; set; }

        public override string ToString() => $"[{machineId}] {GetType().Name} {comment}";
    }
}