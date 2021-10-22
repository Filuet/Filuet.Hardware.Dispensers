using Filuet.ASC.Kiosk.OnBoard.Dispensing.Tests.Entities;
using Filuet.Hardware.Dispensers.Abstractions;
using Filuet.Hardware.Dispensers.Abstractions.Interfaces;
using Filuet.Hardware.Dispensers.Abstractions.Models;
using Filuet.Hardware.Dispensers.Common;
using Filuet.Hardware.Dispensers.Common.Interfaces;
using Filuet.Hardware.Dispensers.Core;
using Filuet.Hardware.Dispensers.Core.Builders;
using Filuet.Hardware.Dispensers.SDK.Jofemar.VisionEsPlus;
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
                .AddCompositeDispenser(sp =>
                {
                    return new CompositeDispenserBuilder()
                        .AddStrategy(sp.GetRequiredService<IDispensingStrategy>())
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
                        .AddPlanogram(() => PoG.Read("[{\"product\": \"0141\", \"routes\":[ { \"r\": \"1/18/0\", \"q\": 5 }, { \"r\": \"1/16/3\", \"q\" : 3} ] }, { \"product\": \"0145\", \"routes\":[ { \"r\": \"2/18/2\", \"q\" : 1}, { \"r\": \"2/16/0\", \"q\" : 5}]}]"))
                        .AddLayout(sp.GetRequiredService<ILayout>())
                        .AddLayoutRouteConverter(sp.GetRequiredService<ILayoutRouteConverter>()).Build();
                },
                sp => new VisionEsPlusEvenDispensingStrategy(sp.GetRequiredService<ILayout>())
                , null
                )
                .AddLayout((sp) =>
                {
                    ILayoutBuilder layoutBuilder = new LayoutBuilder();

                    layoutBuilder.AddMachine<VisionEspMachine, VisionEspTray, VisionEspBelt>(1)
                                    .AddTray(16)
                                        .AddBelt(3).CommitTray()
                                    .AddTray(18)
                                        .AddBelt(0).AddBelt(1).AddBelt(2).AddBelt(3).AddBelt(4).AddBelt(5).CommitTray().CommitMachine()
                                 .AddMachine<VisionEspMachine, VisionEspTray, VisionEspBelt>(2)
                                    .AddTray(11)
                                        .AddBelt(0).AddBelt(1).AddBelt(2).AddBelt(3).AddBelt(4).CommitTray()
                                    .AddTray(16)
                                        .AddBelt(0).CommitTray()
                                    .AddTray(18)
                                        .AddBelt(2).CommitTray().CommitMachine();

                    return layoutBuilder.Build(sp.GetRequiredService<ILayoutRouteConverter>());
                })
                .AddSingleton<ILayoutRouteConverter, VisionEsPlusLayoutRouteConverter>()
                .BuildServiceProvider();

            ICompositeDispenser compositeDispenser = sp.GetRequiredService<ICompositeDispenser>();
            compositeDispenser.Dispense(new (string productUid, ushort quantity)[] { ("0141", 1 ) });

            Console.WriteLine();
        }
    }
}
