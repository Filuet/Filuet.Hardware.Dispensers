using Filuet.Hardware.Dispensers.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Filuet.Hardware.Dispensers.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCompositeDispenser(this IServiceCollection serviceCollection,
            Func<IServiceProvider, ICompositeDispenser> dispenserSetup,
            Func<ICompositeDispenser, ICompositeDispenser> decorator = null)
            => serviceCollection
            .AddSingleton(sp => decorator != null ? decorator(dispenserSetup(sp)) : dispenserSetup(sp)); //TraceDecorator<ICompositeDispenser>.Create(dispenserSetup(sp))
    }
}