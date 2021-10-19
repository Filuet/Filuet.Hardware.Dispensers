using Filuet.Hardware.Dispensers.Abstractions;
using Filuet.Hardware.Dispensers.Abstractions.Interfaces;
using Filuet.Hardware.Dispensers.Abstractions.Models;
using Filuet.Hardware.Dispensers.Core.Builders;
using Filuet.Hardware.Dispensers.Core.Strategy;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Filuet.Hardware.Dispensers.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLayoutBuilder<TTray, TBelt>(this IServiceCollection serviceCollection
                , Action<ILayoutBuilder> builderAction)
            where TTray : Tray, new()
            where TBelt : Belt, new()
        {
            LayoutBuilder builder = new LayoutBuilder();
            builderAction?.Invoke(builder);

            return serviceCollection
                .AddSingleton<ILayoutBuilder>(builder);
        }

        public static IServiceCollection AddCompositeDispenser(this IServiceCollection serviceCollection,
            Func<IServiceProvider, ICompositeDispenser> dispenserSetup,
            Func<ICompositeDispenser, ICompositeDispenser> decorator = null)
            => serviceCollection
            .AddSingleton<IDispensingStrategy, MockDispensingStrategy>()
            .AddSingleton(sp => decorator != null ? decorator(dispenserSetup(sp)) : dispenserSetup(sp)); //TraceDecorator<ICompositeDispenser>.Create(dispenserSetup(sp))

        public static IServiceCollection AddLayout(this IServiceCollection serviceCollection,
            Func<IServiceProvider, ILayout> layoutSetup,
            Func<ILayout, ILayout> decorator = null)
            => serviceCollection.AddSingleton(sp => decorator != null ? decorator(layoutSetup(sp)) : layoutSetup(sp)); //TraceDecorator<ICompositeDispenser>.Create(layoutSetup(sp))
    }
}