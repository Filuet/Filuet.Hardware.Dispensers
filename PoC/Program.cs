using Filuet.Hardware.Dispensers.Abstractions;
using Filuet.Hardware.Dispensers.Abstractions.Models;
using Filuet.Hardware.Dispensers.Core;
using Filuet.Hardware.Dispensers.Core.Strategy;
using Filuet.Hardware.Dispensers.SDK.Jofemar.VisionEsPlus;
using Filuet.Infrastructure.Communication;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace PoC
{
    class Program
    {
        static void Main(string[] args)
        {
            IServiceProvider sp = new ServiceCollection()
                .AddSingleton(PoG.Read(Properties.Resources.test_pranogram))
                .AddCompositeDispenser(sp =>
                {
                    return new CompositeDispenserBuilder()
                    .AddChainBuilder(new DispensingChainBuilder(sp.GetRequiredService<PoG>()))
                        .AddDispensers(() =>
                            {
                                List<IDispenser> result = new List<IDispenser>();

                                VisionEsPlusSettings settings = new VisionEsPlusSettings
                                {
                                    PortNumber = (ushort)5051,
                                    Address = "0x01",
                                    IpAddress = "172.16.7.103"
                                };

                                ICommunicationChannel channel = new TcpChannel(settings.IpAddress, settings.PortNumber);
 
                                result.Add(new VisionEsPlusVendingMachine(1, new VisionEsPlus(channel, settings)));

                                return result;
                            })
                        .AddPlanogram(sp.GetRequiredService<PoG>())
                        .Build();
                }, null)
                .BuildServiceProvider();

            ICompositeDispenser compositeDispenser = sp.GetRequiredService<ICompositeDispenser>();
            compositeDispenser.Dispense(new (string productUid, ushort quantity)[] { ("0141", 1 ) });

            Console.WriteLine();
        }
    }
}
