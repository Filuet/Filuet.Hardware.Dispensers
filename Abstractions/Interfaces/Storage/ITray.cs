using Filuet.Hardware.Dispensers.Abstractions.Models;
using System.Collections.Generic;

namespace Filuet.Hardware.Dispensers.Abstractions.Interfaces
{
    public interface ITray : ILayoutUnit
    {
        IMachine Machine { get; }

        IEnumerable<IBelt> Belts { get; }

        IBelt AddBelt<TBelt>(ushort number)
            where TBelt : Belt, new();
    }
}