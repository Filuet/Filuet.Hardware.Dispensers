using Filuet.Hardware.Dispensers.Abstractions;
using Filuet.Hardware.Dispensers.Abstractions.Models;
using Filuet.Hardware.Dispensers.Abstractions.Interfaces;
using System;
using System.Collections.Generic;

namespace Filuet.Hardware.Dispensers.Core
{
    public class CompositeDispenserBuilder
    {
        public CompositeDispenserBuilder AddStrategy(IDispensingStrategy strategy)
        {
            _strategy = strategy;
            return this;
        }

        public CompositeDispenserBuilder AddDispensers(Func<IEnumerable<IDispenser>> getDispensers)
        {
            _dispensers = getDispensers();
            return this;
        }

        public CompositeDispenserBuilder AddPlanogram(Func<PoG> getPlanogram)
        {
            _planogram = getPlanogram();
            return this;
        }

        public ICompositeDispenser Build()
        {
            return new CompositeDispenser(_dispensers, _strategy, _planogram);
        }

        private IEnumerable<IDispenser> _dispensers;
        private IDispensingStrategy _strategy;
        private PoG _planogram;
    }
}
