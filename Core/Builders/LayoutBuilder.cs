using Filuet.Hardware.Dispensers.Abstractions.Interfaces;
using Filuet.Hardware.Dispensers.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Filuet.Hardware.Dispensers.Core.Builders
{
    public sealed class LayoutBuilder : ILayoutBuilder  
    {
        private IList<IMachine> _machines = new List<IMachine>();

        public ILayoutBuilderMachine<TTray, TBelt> AddMachine<TMachine, TTray, TBelt>(ushort number)
            where TMachine : Machine, new()
            where TTray : Tray, new()
            where TBelt : Belt, new()
        {
            IMachine machine = CreateMachine<TMachine>(number);
            return new LayoutBuilderMachine<TTray, TBelt>(this, machine);
        }

        private IMachine CreateMachine<TMachine>(ushort number)
            where TMachine : Machine, new()
        {
            IMachine machine = _machines.SingleOrDefault(x => x.Number == number);
            if (machine == null)
            {
                machine = new TMachine();
                machine.SetNumber(number);
                _machines.Add(machine);
            }

            return machine;
        }

        public Layout Build(ILayoutRouteConverter routeConverter, Func<ILayout, bool> validate = null)
        {
            Layout layout = new Layout(routeConverter);
            layout.AddMachines(_machines);
            
            return (validate == null || validate(layout)) ? layout : null;
        }
    }
}