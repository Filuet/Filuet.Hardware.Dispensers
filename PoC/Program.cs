using Filuet.Hardware.Dispensers.Abstractions;
using Filuet.Hardware.Dispensers.Abstractions.Models;
using Filuet.Hardware.Dispensers.Core;
using Filuet.Hardware.Dispensers.SDK.Jofemar.VisionEsPlus;
using Filuet.Hardware.Dispensers.SDK.Jofemar.VisionEsPlus.Communication;
using Filuet.Infrastructure.Communication;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Filuet.Infrastructure.DataProvider.Interfaces;
using Filuet.Infrastructure.DataProvider;
using Microsoft.Extensions.Logging;

namespace PoC
{
    class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        const string planogram_path = "C:/Filuet/Dispensing/test_planogram.json";

        static void Main(string[] args) {
            AllocConsole();

            PoCForm form = new PoCForm();

            form.Show();

            IServiceProvider sp = new ServiceCollection()
                .AddTransient(sp => Pog.Read(File.ReadAllText(planogram_path)))
                .AddSingleton<IMemoryCachingService, MemoryCachingService>()
                .AddVendingMachine(sp => {
                    ICollection<ILightEmitter> integratedEmitters = new List<ILightEmitter>();

                    IVendingMachine vendingMachine = new VendingMachineBuilder()
                        .AddDispensers(() => {
                            List<IDispenser> result = new List<IDispenser>();
                            #region Machine1
                            VisionEsPlusSettings settings1 = new VisionEsPlusSettings {
                                Id = 1,
                                Emulation = true,
                                PortNumber = 5050,
                                Address = string.Format("0x{0:X2}", 1), // "0x01",
                                IpOrSerialAddress = "COM11",// "172.16.7.104",//
                                LightSettings = new VisionEsPlusLightEmitterSettings { LightsAreNormallyOn = true },
                                PollFrequencyHz = 0.33m,
                                MaxExtractWeightPerTime = 1200
                            };

                            ICommunicationChannel channel1 = new EspSerialChannel(s => { s.PortName = settings1.IpOrSerialAddress; });
                            // new EspTcpChannel(s => { s.Endpoint = new IPEndPoint(IPAddress.Parse(settings1.IpOrSerialAddress), settings1.PortNumber); });

                            VisionEsPlusEmulationCache emulatorCache1 = settings1.Emulation ? new VisionEsPlusEmulationCache(sp.GetRequiredService<IMemoryCachingService>().Get($"MachineEmulator1", 1)) : null;
                            VisionEsPlusWrapper machine1 = new VisionEsPlusWrapper(new VisionEsPlus(channel1, settings1, () => sp.GetService<Pog>(), emulatorCache1));
                            result.Add(machine1);
                            integratedEmitters.Add(machine1);
                            #endregion

                            //Uncomment to enable machine2
                            #region Machine2
                            VisionEsPlusSettings settings2 = new VisionEsPlusSettings {
                                Id = 2,
                                Emulation = true,
                                PortNumber = 5051,
                                Address = string.Format("0x{0:X2}", 1), // "0x01",
                                IpOrSerialAddress = "COM9",// "172.16.7.103",
                                LightSettings = new VisionEsPlusLightEmitterSettings { LightsAreNormallyOn = true },
                                PollFrequencyHz = 1m
                            };

                            ICommunicationChannel channel2 = new EspSerialChannel(s => { s.PortName = settings2.IpOrSerialAddress; }); // new EspTcpChannel(s => { s.Endpoint = new IPEndPoint(IPAddress.Parse(settings2.IpOrSerialAddress), settings2.PortNumber); });

                            VisionEsPlusEmulationCache emulatorCache2 = settings2.Emulation ? new VisionEsPlusEmulationCache(sp.GetRequiredService<IMemoryCachingService>().Get($"MachineEmulator2", 1)) : null;
                            VisionEsPlusWrapper machine2 = new VisionEsPlusWrapper(new VisionEsPlus(channel2, settings2, () => sp.GetService<Pog>(), emulatorCache2));
                            result.Add(machine2);
                            integratedEmitters.Add(machine2);
                            #endregion

                            return result;
                        })
                        .AddLightEmitters(() => integratedEmitters)
                        .AddPlanogram(sp.GetRequiredService<Pog>())
                        .Build();

                    vendingMachine.onDispensing += (sender, e) => form.Log(LogLevel.Information, e.ToString(), e.sessionId);
                    vendingMachine.onDispensed += (sender, e) => form.Log(LogLevel.Information, e.ToString(), e.sessionId);
                    vendingMachine.onAbandonment += (sender, e) => form.Log(LogLevel.Warning, e.ToString() + "@", e.sessionId);
                    vendingMachine.onFailed += (sender, e) => form.Log(LogLevel.Error, e.ToString(), e.sessionId);
                    vendingMachine.onLightsChanged += (sender, e) => form.Log(LogLevel.Information, $"Machine {e.Id} Lights are {(e.IsOn ? "On" : "Off")}");
                    vendingMachine.onPlanogramClarification += (sender, e) => {
                        form.Planogram = e.planogram;
                        form.Log(LogLevel.Information, e.ToString() ?? "The planogram refreshed", e.sessionId);
                        e.planogram.Write(planogram_path);
                    };
                    vendingMachine.onAddressInactive += (sender, e) => form.Log(LogLevel.Warning, e.ToString(), e.sessionId);

                    // vendingMachine.onTest += (sender, e) => form.Log(Microsoft.IdentityModel.Clients.ActiveDirectory.LogLevel.Information, e.Message);
                    ////vendingMachine.onResponse += (sender, e) => Console.WriteLine($"{sender}: {e}");
                    return vendingMachine;
                }, null)
                .BuildServiceProvider();

            form.Initialize(sp.GetRequiredService<Pog>(), sp.GetRequiredService<IVendingMachine>());

            Application.Run(form);
        }
    }
}