using Filuet.Hardware.Dispensers.Abstractions;
using Filuet.Hardware.Dispensers.Abstractions.Models;
using Filuet.Hardware.Dispensers.Core.Strategy;
using System;
using System.Collections.Generic;

namespace Filuet.Hardware.Dispensers.Core
{
    public class VendingMachineBuilder
    {
        public VendingMachineBuilder AddChainBuilder(DispensingChainBuilder chainBuilder)
        {
            _chainBuilder = chainBuilder;
            return this;
        }

        public VendingMachineBuilder AddDispensers(Func<IEnumerable<IDispenser>> getDispensers)
        {
            _dispensers = getDispensers();
            return this;
        }

        /// <summary>
        /// Just in case, if there are some non-integrated light emitters
        /// </summary>
        /// <param name="getLightEmitter"></param>
        /// <returns></returns>
        public VendingMachineBuilder AddLightEmitters(Func<IEnumerable<ILightEmitter>> getLightEmitter)
        {
            _lightEmitters = getLightEmitter();
            return this;
        }

        public VendingMachineBuilder AddPlanogram(PoG planogram)
        {
            _planogram = planogram;
            return this;
        }

        public IVendingMachine Build()
            => new VendingMachine(_dispensers, _lightEmitters, _chainBuilder, _planogram);

        private IEnumerable<IDispenser> _dispensers;
        private IEnumerable<ILightEmitter> _lightEmitters;
        private DispensingChainBuilder _chainBuilder;
        private PoG _planogram;
    }
}