using Filuet.Hardware.Dispensers.Abstractions.Models;
using System;

namespace Filuet.Hardware.Dispensers.Abstractions.Interfaces
{
    public interface ILayoutBuilderMachine<TTray, TBelt>
        where TTray : Tray, new()
        where TBelt : Belt, new()
    {
        ILayoutBuilderTray<TTray, TBelt> AddTray(ushort number);

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
        ILayoutBuilderTray<TTray, TBelt> AddBelt(ushort number);

        ILayoutBuilderMachine<TTray, TBelt> CommitTray();
    }

    public interface ILayoutBuilder
    {
        ILayoutBuilderMachine<TTray, TBelt> AddMachine<TMachine, TTray, TBelt>(ushort number)
            where TMachine : Machine, new()
            where TTray : Tray, new()
            where TBelt : Belt, new();

        Layout Build(ILayoutRouteConverter routeConverter, Func<ILayout, bool> validate = null);
    }
}
