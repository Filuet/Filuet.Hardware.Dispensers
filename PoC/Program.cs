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
using Filuet.Hardware.Dispensers.SDK.Jofemar.VisionEsPlus.Communication;

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
                .AddVendingMachine(sp =>
                {
                    ICollection<ILightEmitter> integratedEmitters = new List<ILightEmitter>(); 

                    IVendingMachine vendingMachine = new VendingMachineBuilder()
                    .AddChainBuilder(new DispensingChainBuilder(sp.GetRequiredService<PoG>()))
                        .AddDispensers(() =>
                            {
                                List<IDispenser> result = new List<IDispenser>();

                                #region Machine1
                                VisionEsPlusSettings settings1 = new VisionEsPlusSettings
                                {
                                    PortNumber = (ushort)5050,
                                    Address = string.Format("0x{0:X2}", 1), // "0x01",
                                    IpAddress = "172.16.7.103",
                                    LightSettings = new VisionEsPlusLightEmitterSettings { LightsAreNormallyOn = true }
                                };

                                ICommunicationChannel channel1 = new EspTcpChannel(s => { s.Endpoint = new IPEndPoint(IPAddress.Parse(settings1.IpAddress), settings1.PortNumber); });

                                VisionEsPlusVendingMachine machine1 = new VisionEsPlusVendingMachine(1, new VisionEsPlus(channel1, settings1));
                                result.Add(machine1);
                                integratedEmitters.Add(machine1);
                                #endregion

                                #region Machine2
                                VisionEsPlusSettings settings2 = new VisionEsPlusSettings
                                {
                                    PortNumber = (ushort)5051,
                                    Address = string.Format("0x{0:X2}", 1), // "0x01",
                                    IpAddress = "172.16.7.103"
                                };

                                ICommunicationChannel channel2 = new EspTcpChannel(s => { s.Endpoint = new IPEndPoint(IPAddress.Parse(settings2.IpAddress), settings2.PortNumber); });

                                VisionEsPlusVendingMachine machine2 = new VisionEsPlusVendingMachine(2, new VisionEsPlus(channel2, settings2));
                                result.Add(machine2);
                                integratedEmitters.Add(machine2);
                                #endregion

                                return result;
                            })
                        .AddLightEmitters(() => integratedEmitters)
                        .AddPlanogram(sp.GetRequiredService<PoG>())
                        .Build();

                    vendingMachine.onDispensed += (sender, e) => form.Log(Microsoft.IdentityModel.Clients.ActiveDirectory.LogLevel.Information, $"Dispensing started {e.address}");
                    vendingMachine.onDispensingFinished += (sender, e) => form.Log(Microsoft.IdentityModel.Clients.ActiveDirectory.LogLevel.Information, $"Dispensing finished {e}");
                    vendingMachine.onTest += (sender, e) => form.Log(Microsoft.IdentityModel.Clients.ActiveDirectory.LogLevel.Information, $"Dispensing finished {e.Message}");
                    ////vendingMachine.onResponse += (sender, e) => Console.WriteLine($"{sender}: {e}");
                    return vendingMachine;
                }, null)
                .BuildServiceProvider();

            form.Initialize(sp.GetRequiredService<PoG>(), sp.GetRequiredService<IVendingMachine>());

            Application.Run(form);
        }
    }
}