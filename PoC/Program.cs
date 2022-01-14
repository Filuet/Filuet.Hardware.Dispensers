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
using System.Runtime.InteropServices;
using System.Net;

namespace PoC
{
    class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        static void Main(string[] args)
        {
            PoCForm form = new PoCForm();

            IServiceProvider sp = new ServiceCollection()
                .AddSingleton(PoG.Read(File.ReadAllText("test_planogram.json")))
                .AddCompositeDispenser(sp =>
                {
                    ICompositeDispenser compositeDispenser = new CompositeDispenserBuilder()
                    .AddChainBuilder(new DispensingChainBuilder(sp.GetRequiredService<PoG>()))
                        .AddDispensers(() =>
                            {
                                List<IDispenser> result = new List<IDispenser>();

                                for (int i = 1; i <= 1/* SET 2 to attach the machine № 2 */; i++) 
                                {
                                    VisionEsPlusSettings settings = new VisionEsPlusSettings
                                    {
                                        PortNumber = (ushort)5051,
                                        Address = string.Format("0x{0:X2}", i), // "0x01",
                                        IpAddress = "172.16.7.103"
                                    };

                                    ICommunicationChannel channel = new TcpChannel(x => { x.Endpoint = new System.Net.IPEndPoint(IPAddress.Parse(settings.IpAddress), settings.PortNumber); });

                                    result.Add(new VisionEsPlusVendingMachine((uint)i, new VisionEsPlus(channel, settings)));
                                }

                                return result;
                            })
                        .AddPlanogram(sp.GetRequiredService<PoG>())
                        .Build();

                    compositeDispenser.onDispensing += (sender, e) => form.Log(Microsoft.IdentityModel.Clients.ActiveDirectory.LogLevel.Information, $"Dispensing started {e.address}");
                    compositeDispenser.onDispensingFinished += (sender, e) => form.Log(Microsoft.IdentityModel.Clients.ActiveDirectory.LogLevel.Information, $"Dispensing finished {e}");
                    compositeDispenser.onTest += (sender, e) => form.Log(Microsoft.IdentityModel.Clients.ActiveDirectory.LogLevel.Information, $"Dispensing finished {e.Message}");
                    ////compositeDispenser.onResponse += (sender, e) => Console.WriteLine($"{sender}: {e}");
                    return compositeDispenser;
                }, null)
                .BuildServiceProvider();

            form.Initialize(sp.GetRequiredService<PoG>(), sp.GetRequiredService<ICompositeDispenser>());
            AllocConsole();
            Application.Run(form);
        }
    }
}