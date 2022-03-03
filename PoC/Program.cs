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
            AllocConsole();

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

                                #region Machine1
                                VisionEsPlusSettings settings1 = new VisionEsPlusSettings
                                {
                                    PortNumber = (ushort)5050,
                                    Address = string.Format("0x{0:X2}", 1), // "0x01",
                                    IpAddress = "172.16.7.103"
                                };

                                ICommunicationChannel channel1 = new TcpChannel(s => { s.Endpoint = new IPEndPoint(IPAddress.Parse(settings1.IpAddress), settings1.PortNumber);
                                    s.ReadDelay = TimeSpan.FromMilliseconds(50);  });

                                result.Add(new VisionEsPlusVendingMachine(1, new VisionEsPlus(channel1, settings1)));
                                #endregion

                                #region Machine2
                                VisionEsPlusSettings settings2 = new VisionEsPlusSettings
                                {
                                    PortNumber = (ushort)5051,
                                    Address = string.Format("0x{0:X2}", 1), // "0x01",
                                    IpAddress = "172.16.7.103"
                                };

                                ICommunicationChannel channel2 = new TcpChannel(s => { s.Endpoint = new IPEndPoint(IPAddress.Parse(settings2.IpAddress), settings2.PortNumber); });

                                result.Add(new VisionEsPlusVendingMachine(2, new VisionEsPlus(channel2, settings2)));
                                #endregion

                                return result;
                            })
                        .AddPlanogram(sp.GetRequiredService<PoG>())
                        .Build();

                    compositeDispenser.onDispensed += (sender, e) => form.Log(Microsoft.IdentityModel.Clients.ActiveDirectory.LogLevel.Information, $"Dispensing started {e.address}");
                    compositeDispenser.onDispensingFinished += (sender, e) => form.Log(Microsoft.IdentityModel.Clients.ActiveDirectory.LogLevel.Information, $"Dispensing finished {e}");
                    compositeDispenser.onTest += (sender, e) => form.Log(Microsoft.IdentityModel.Clients.ActiveDirectory.LogLevel.Information, $"Dispensing finished {e.Message}");
                    ////compositeDispenser.onResponse += (sender, e) => Console.WriteLine($"{sender}: {e}");
                    return compositeDispenser;
                }, null)
                .BuildServiceProvider();

            form.Initialize(sp.GetRequiredService<PoG>(), sp.GetRequiredService<ICompositeDispenser>());

            Application.Run(form);
        }
    }
}