using Filuet.ASC.Kiosk.OnBoard.Dispensing.Abstractions.Models;
using System.Collections.Generic;

namespace Filuet.ASC.Kiosk.OnBoard.Dispensing.Abstractions.Interfaces
{
    public interface IMachine : ILayoutUnit
    {
        IEnumerable<ITray> Trays { get; }

        ITray AddTray<TTray>(uint number)
            where TTray : Tray, new();
    }
}