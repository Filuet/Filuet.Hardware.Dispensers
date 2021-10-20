namespace Filuet.Hardware.Dispensers.Abstractions.Interfaces
{
    public interface ILayoutUnit
    {
        /// <summary>
        /// Index number
        /// </summary>
        ushort Number { get; }

        bool IsActive { get; }

        void SetNumber(ushort number);

        void SetActive(bool active);
    }
}