using Filuet.Hardware.Dispensers.Abstractions;
using Filuet.Hardware.Dispensers.Abstractions.Models;
using Filuet.Hardware.Dispensers.Core.Strategy;
using System;
using System.Collections.Generic;

namespace Filuet.Hardware.Dispensers.Core
{
    public class CompositeDispenserBuilder
    {
        public CompositeDispenserBuilder AddChainBuilder(DispensingChainBuilder chainBuilder)
        {
            _chainBuilder = chainBuilder;
            return this;
        }

        public CompositeDispenserBuilder AddDispensers(Func<IEnumerable<IDispenser>> getDispensers)
        {
            _dispensers = getDispensers();
            return this;
        }

        public CompositeDispenserBuilder AddPlanogram(PoG planogram)
        {
            _planogram = planogram;
            return this;
        }

        public ICompositeDispenser Build()
            => new CompositeDispenser(_dispensers, _chainBuilder, _planogram);

        private IEnumerable<IDispenser> _dispensers;
        private DispensingChainBuilder _chainBuilder;
        private PoG _planogram;
    }
}