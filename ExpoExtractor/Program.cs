using ExpoExtractor;
using Filuet.Hardware.Dispensers.Abstractions;
using Filuet.Hardware.Dispensers.Abstractions.Models;
using Filuet.Hardware.Dispensers.Core;
using Filuet.Hardware.Dispensers.SDK.Jofemar.VisionEsPlus;
using Filuet.Hardware.Dispensers.SDK.Jofemar.VisionEsPlus.Communication;
using Filuet.Infrastructure.Communication;
using Filuet.Infrastructure.DataProvider.Interfaces;
using Filuet.Infrastructure.DataProvider;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string planogramAddress = "C:/Filuet/Dispensing/test_planogram.json";

Pog planogram = Pog.Read(File.ReadAllText(planogramAddress));
builder.Services.AddSingleton(planogram)
    .AddSingleton<IMemoryCachingService, MemoryCachingService>()
    .AddVendingMachine(sp => {
        ICollection<ILightEmitter> integratedEmitters = new List<ILightEmitter>();
        IVendingMachine vendingMachine = new VendingMachineBuilder()
            .AddDispensers(() => {
                string jsonSettings = File.ReadAllText("C:/Filuet/Dispensing/dispensing_settings.json");
                var machineSettings = JsonSerializer.Deserialize<IEnumerable<VisionEsPlusSettings>>(jsonSettings);
                List<IDispenser> result = new List<IDispenser>();
                
                foreach (var settings in machineSettings) {
                    ICommunicationChannel channel = settings.IpOrSerialAddress.Contains("COM") ?
                        new EspSerialChannel(s => { s.PortName = settings.IpOrSerialAddress; }) :
                        new EspTcpChannel(s => { s.Endpoint = new IPEndPoint(IPAddress.Parse(settings.IpOrSerialAddress), settings.PortNumber); });

                    VisionEsPlusEmulationCache emulatorCache = settings.Emulation ? new VisionEsPlusEmulationCache(sp.GetRequiredService<IMemoryCachingService>().Get($"MachineEmulator{settings.Id}", 1)) : null;
                    VisionEsPlusWrapper machine = new VisionEsPlusWrapper(new VisionEsPlus(channel, settings, () => sp.GetService<Pog>(), emulatorCache));
                    result.Add(machine);
                    integratedEmitters.Add(machine);
                }

                return result;
            })
            .AddLightEmitters(() => integratedEmitters)
            .AddPlanogram(sp.GetRequiredService<Pog>())
            .Build();

        vendingMachine.onDispensing += (sender, e) => {
            StatusSingleton.Status = new CurrentStatus { Action = "dispensing", Status = "success", Message = $"{e.address} Dispensing started" };
            Console.WriteLine($"{e.address} Dispensing started");
        };
        vendingMachine.onDispensed += (sender, e) => {
            StatusSingleton.Status = new CurrentStatus { Action = "dispensed", Status = "success", Message = $"{e.address} Dispensing completed. You can carry on with dispensing" };
            Console.WriteLine($"{e.address} Dispensing completed. You can carry on with dispensing");
        };
        vendingMachine.onAbandonment += (sender, e) => {
            StatusSingleton.Status = new CurrentStatus { Action = "dispensing", Status = "failed", Message = $"Likely that products were abandoned {e}" };
            Console.WriteLine($"Likely that products were abandoned {e}");
        };
        vendingMachine.onFailed += (sender, e) => {
            StatusSingleton.Status = new CurrentStatus { Action = "dispensing", Status = "failed", Message = e.ToString() };
            Console.WriteLine(e.ToString());
        };

        vendingMachine.onLightsChanged += (sender, e) => {
            StatusSingleton.Status = new CurrentStatus { Action = "lights", Status = "success", Message = $"Machine {e.Id} Lights are {(e.IsOn ? "On" : "Off")}" };
            Console.WriteLine($"Machine {e.Id} Lights are {(e.IsOn ? "On" : "Off")}");
        };

        vendingMachine.onMachineUnlocked += (sender, e) => {
            StatusSingleton.Status = new CurrentStatus { Action = "unlock", Status = "success", Message = $"{e.machine} is unlocked" };
            Console.WriteLine($"{e.machine} is unlocked");
        };

        vendingMachine.onWaitingProductsToBeRemoved += (sender, e) => {
            StatusSingleton.Status = new CurrentStatus { Action = "takeproducts", Status = "success", Message = $"Dispenser is waiting for products to be removed" };
            Console.WriteLine($"{DateTime.Now:HH:mm:ss}: Dispenser is waiting for products to be removed");
        };

        vendingMachine.onPlanogramClarification += (sender, e) => {
            e.Planogram.Write(planogramAddress);
            //form.Planogram = e.Planogram;
            //form.Log(Microsoft.IdentityModel.Clients.ActiveDirectory.LogLevel.Information, $"The planogram is downloaded");
        };
        // vendingMachine.onTest += (sender, e) => form.Log(Microsoft.IdentityModel.Clients.ActiveDirectory.LogLevel.Information, e.Message);
        ////vendingMachine.onResponse += (sender, e) => Console.WriteLine($"{sender}: {e}");

        return vendingMachine;
    }, null);

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment()) {
app.UseSwagger();
app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
