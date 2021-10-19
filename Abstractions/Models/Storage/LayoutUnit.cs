using Filuet.Hardware.Dispensers.Abstractions.Interfaces;

namespace Filuet.Hardware.Dispensers.Abstractions.Models
{
    public abstract class LayoutUnit : ILayoutUnit
    {
        public uint Number { get; private set; }

        public bool IsActive { get; private set; } = true;

        public void SetActive(bool active) => IsActive = active;

        public void SetNumber(uint number) => Number = number;
    }
}