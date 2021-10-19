using Filuet.ASC.Kiosk.OnBoard.Dispensing.Abstractions.Interfaces;

namespace Filuet.ASC.Kiosk.OnBoard.Dispensing.Abstractions.Models
{
    public abstract class LayoutUnit : ILayoutUnit
    {
        public uint Number { get; private set; }

        public bool IsActive { get; private set; } = true;

        public void SetActive(bool active) => IsActive = active;

        public void SetNumber(uint number) => Number = number;
    }
}