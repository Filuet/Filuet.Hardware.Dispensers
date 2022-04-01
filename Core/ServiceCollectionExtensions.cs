using Filuet.Hardware.Dispensers.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Filuet.Hardware.Dispensers.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddVendingMachine(this IServiceCollection serviceCollection,
            Func<IServiceProvider, IVendingMachine> dispenserSetup,
            Func<IVendingMachine, IVendingMachine> decorator = null)
            => serviceCollection
            .AddSingleton(sp => decorator != null ? decorator(dispenserSetup(sp)) : dispenserSetup(sp)); //TraceDecorator<IVendingMachine>.Create(dispenserSetup(sp))
    }
}