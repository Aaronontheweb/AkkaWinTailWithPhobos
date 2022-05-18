using System;
using System.IO;
using Akka.Actor;
using Akka.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Azure.Monitor.OpenTelemetry.Exporter;
using Phobos.Actor;
using Phobos.Actor.Configuration;
using WinTail;

var assemblyName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
var actorSystemName = "WinTailActorSystem";
var serviceName = "WinTail";
var serviceVersion = "1.0.0";

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddOpenTelemetryTracing();
        services.AddHostedService<ConsoleService>();
        services.AddSingleton<ActorSystem>(sp =>
        {
            var hocon = ConfigurationFactory.ParseString(File.ReadAllText("config/phobos.hocon"));
            var setup = PhobosSetup.Create(
                    new PhobosConfigBuilder()
                        .WithTracing(tb =>
                        {
                            Sdk.CreateTracerProviderBuilder()
                                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName, serviceVersion: serviceVersion))
                                .AddPhobosInstrumentation()
                                .AddJaegerExporter()
                                .AddOtlpExporter(configure =>
                                {
                                    configure.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                                    configure.Endpoint =
                                        new Uri(
                                            "https://608cdbaa68ff4f24a508ad702ed9dd9a.apm.westeurope.azure.elastic-cloud.com:443");
                                    configure.Headers = "Authorization=Bearer LJxGdA2iz0L4WMhw98";
                                })
                                .AddAzureMonitorTraceExporter(o => o.ConnectionString = "InstrumentationKey=ff70c9d2-d463-4e98-acb2-b1862bbddde1;IngestionEndpoint=https://westeurope-2.in.applicationinsights.azure.com/")
                                .Build();
                        }))
                .WithSetup(BootstrapSetup.Create().WithConfig(hocon).WithActorRefProvider(PhobosProviderSelection.Local));
            return ActorSystem.Create(actorSystemName, setup);
        });
    });

var host = builder.Build();

Console.WriteLine("Write whatever you want into the console!");
Console.Write("Some lines will appear as");
Console.ForegroundColor = ConsoleColor.DarkRed;
Console.Write(" red ");
Console.ResetColor();
Console.Write(" and others will appear as");
Console.ForegroundColor = ConsoleColor.Green;
Console.Write(" green! ");
Console.ResetColor();
Console.WriteLine();
Console.WriteLine();
Console.WriteLine("Type 'exit' to quit this application at any time.\n");

await host.RunAsync();
