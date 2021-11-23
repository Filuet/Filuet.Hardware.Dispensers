using Filuet.Hardware.Dispensers.Abstractions;
using Filuet.Hardware.Dispensers.Abstractions.Models;
using Filuet.Hardware.Dispensers.Core;
using Filuet.Hardware.Dispensers.Core.Strategy;
using Filuet.Hardware.Dispensers.SDK.Jofemar.VisionEsPlus;
using Filuet.Infrastructure.Communication;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace PoC
{
    class Program
    {
        static void Main(string[] args)
        {
            IServiceProvider sp = new ServiceCollection()
                .AddSingleton(PoG.Read(File.ReadAllText("test_planogram.json")))
                .AddCompositeDispenser(sp =>
                {
                    return new CompositeDispenserBuilder()
                    .AddChainBuilder(new DispensingChainBuilder(sp.GetRequiredService<PoG>()))
                        .AddDispensers(() =>
                            {
                                List<IDispenser> result = new List<IDispenser>();

                                for (int i = 1; i <= 2; i++)
                                {
                                    VisionEsPlusSettings settings = new VisionEsPlusSettings
                                    {
                                        PortNumber = (ushort)5051,
                                        Address = string.Format("0x{0:X2}", i), // "0x01",
                                        IpAddress = "172.16.7.103"
                                    };

                                    ICommunicationChannel channel = new TcpChannel(settings.IpAddress, settings.PortNumber);

                                    result.Add(new VisionEsPlusVendingMachine((uint)i, new VisionEsPlus(channel, settings)));
                                }

                                return result;
                            })
                        .AddPlanogram(sp.GetRequiredService<PoG>())
                        .Build();
                }, null)
                .BuildServiceProvider();

            PoCForm form = new PoCForm();
            form.Initialize(sp.GetRequiredService<PoG>(), sp.GetRequiredService<ICompositeDispenser>());
            Application.Run(form);
        }
    }
}