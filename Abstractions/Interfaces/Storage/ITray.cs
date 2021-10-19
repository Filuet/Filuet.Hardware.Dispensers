using Filuet.ASC.Kiosk.OnBoard.Dispensing.Abstractions.Models;
using System.Collections.Generic;

namespace Filuet.ASC.Kiosk.OnBoard.Dispensing.Abstractions.Interfaces
{
    public interface ITray : ILayoutUnit
    {
        IEnumerable<IBelt> Belts { get; }

        IBelt AddBelt<TBelt>(uint number)
            where TBelt : Belt, new();
    }
}