using Filuet.Hardware.Dispenser;
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
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);
// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console(outputTemplate: "{HH:mm:ss.fff zzz} [{Level:u}] [{SourceContext}] {Message}{NewLine}{Exception}")
    .WriteTo.File(
        path: builder.Configuration["LocalDispenserLogsPath"],
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u}] [{SourceContext}] {Message}{NewLine}{Exception}"
    )
    .WriteTo.AzureBlobStorage(
        connectionString: builder.Configuration["AzureBlobConnectionString"],
        storageContainerName: builder.Configuration["AzureBlobLogsContainerName"],
        storageFileName: $"log-{DateTime.UtcNow.ToString("yyyy-MM-dd")}.txt")
    .CreateLogger();
builder.Host.UseSerilog();
builder.Logging.AddSerilog();
// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string planogramAddress = builder.Configuration["PlanogramPath"];
ILogger<Program> logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();


builder.Services.AddSingleton(sp =>
    new PlanogramService(sp.GetRequiredService<IConfiguration>(), sp, sp.GetRequiredService<ILogger<PlanogramService>>())
);
builder.Services.AddSingleton(sp => sp.GetRequiredService<PlanogramService>().GetPlanogram())
    .AddSingleton<IMemoryCachingService, MemoryCachingService>()
    .AddVendingMachine(sp =>
    {
        ICollection<ILightEmitter> integratedEmitters = new List<ILightEmitter>();

        IVendingMachine vendingMachine = new VendingMachineBuilder()
            .AddDispensers(() =>
            {
                string jsonSettings = File.ReadAllText(builder.Configuration["DispensingSettingsPath"]);
                var machineSettings = JsonSerializer.Deserialize<IEnumerable<VisionEsPlusSettings>>(jsonSettings);
                List<IDispenser> result = new List<IDispenser>();

                foreach (var settings in machineSettings)
                {
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
            .AddPlanogram(sp.GetRequiredService<PlanogramService>().GetPlanogram())
            .AddLogger(sp.GetRequiredService<ILogger<VendingMachine>>())
            .Build();

        vendingMachine.onDispensing += (sender, e) =>
        {
            StatusSingleton.Status = new CurrentStatus { Action = "dispensing", Status = "success", Message = e.message };
            Console.WriteLine($"{e.address} Dispensing started");
            logger.LogInformation($"{e.address} Dispensing started");
        };
        vendingMachine.onDispensed += (sender, e) =>
        {
            StatusSingleton.Status = new CurrentStatus { Action = "dispensed", Status = "success", Message = e.message };
            Console.WriteLine($"{e.address} Dispensing completed. You can carry on with dispensing");
            logger.LogInformation($"{e.address} Dispensing completed. You can carry on with dispensing");
        };

        vendingMachine.onDispensedFromUnit += (sender, e) =>
        {
            // dispensing finished from e.Dispenser.Id
            StatusSingleton.Status = new CurrentStatus { Action = "pending", Status = "success", Message = $"Dispensing from unit #{e.Dispenser.Id} finished" };
            Console.WriteLine($"Dispensing from unit #{e.Dispenser.Id} finished");
        };

        vendingMachine.onDispensingFinished += (sender, e) =>
        {
            StatusSingleton.Status = new CurrentStatus { Action = "pending", Status = "success", Message = "Waiting for command" };
        };

        vendingMachine.onAbandonment += (sender, e) =>
        {
            StatusSingleton.Status = new CurrentStatus { Action = "dispensing", Status = "failed", Message = $"Likely that products were abandoned {e}" };
            Console.WriteLine($"Likely that products were abandoned {e}");
            logger.LogInformation($"Likely that products were abandoned {e}");

        };
        vendingMachine.onFailed += (sender, e) =>
        {
            StatusSingleton.Status = new CurrentStatus { Action = "dispensing", Status = "failed", Message = e.ToString() };
            Console.WriteLine(e.ToString());
            logger.LogInformation(e.ToString());

        };

        vendingMachine.onLightsChanged += (sender, e) =>
        {
            StatusSingleton.Status = new CurrentStatus { Action = "lights", Status = "success", Message = $"Machine {e.Id} Lights are {(e.IsOn ? "On" : "Off")}" };
            Console.WriteLine($"Machine {e.Id} Lights are {(e.IsOn ? "On" : "Off")}");
            logger.LogInformation($"Machine {e.Id} Lights are {(e.IsOn ? "On" : "Off")}");

        };

        vendingMachine.onMachineUnlocked += (sender, e) =>
        {
            StatusSingleton.Status = new CurrentStatus { Action = "unlock", Status = "success", Message = $"{e.machine} is unlocked" };
            Console.WriteLine($"{e.machine} is unlocked");
            logger.LogInformation($"{e.machine} is unlocked");

        };

        vendingMachine.onWaitingProductsToBeRemoved += (sender, e) =>
        {
            StatusSingleton.Status = new CurrentStatus { Action = "takeproducts", Status = "success", Message = $"Dispenser is waiting for products to be removed" };
            Console.WriteLine($"{DateTime.Now:HH:mm:ss}: Dispenser is waiting for products to be removed");
            logger.LogInformation($"{DateTime.Now:HH:mm:ss}: Dispenser is waiting for products to be removed");
        };

        vendingMachine.onPlanogramClarification += (sender, e) =>
        {
            e.planogram.Write(planogramAddress);
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
