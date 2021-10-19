using Filuet.ASC.Kiosk.OnBoard.Dispensing.Abstractions.Models;
using System;

namespace Filuet.ASC.Kiosk.OnBoard.Dispensing.Abstractions.Interfaces
{
    public interface ILayoutBuilderMachine<TTray, TBelt>
        where TTray : Tray, new()
        where TBelt : Belt, new()
    {
        ILayoutBuilderTray<TTray, TBelt> AddTray(uint number);

        ILayoutBuilder CommitMachine();
    }

    public interface ILayoutBuilderTray<TTray, TBelt>
        where TTray : Tray, new()
        where TBelt : Belt, new()
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="number">Tray number</param>
        /// <returns></returns>
        ILayoutBuilderTray<TTray, TBelt> AddBelt(uint number);

        ILayoutBuilderMachine<TTray, TBelt> CommitTray();
    }

    public interface ILayoutBuilder
    {
        ILayoutBuilderMachine<TTray, TBelt> AddMachine<TMachine, TTray, TBelt>(uint number)
            where TMachine : Machine, new()
            where TTray : Tray, new()
            where TBelt : Belt, new();

        Layout Build(Func<ILayout, bool> validate = null);
    }
}
