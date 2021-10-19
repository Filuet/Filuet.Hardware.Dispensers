using Filuet.Hardware.Dispensers.Abstractions.Models;
using System.Collections.Generic;

namespace Filuet.Hardware.Dispensers.Abstractions.Interfaces
{
    public interface IMachine : ILayoutUnit
    {
        IEnumerable<ITray> Trays { get; }

        ITray AddTray<TTray>(uint number)
            where TTray : Tray, new();
    }
}