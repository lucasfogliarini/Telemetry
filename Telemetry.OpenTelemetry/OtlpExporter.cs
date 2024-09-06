using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;

namespace OpenTelemetry
{
    public abstract class OtlpExporter(IHostApplicationBuilder builder)
    {
        protected IHostApplicationBuilder Builder { get; private set; } = builder;

        public void Build()
        {
            var resourceBuilder = ConfigureResourceBuilder();
            Builder
                .Services
                .AddOpenTelemetry()
                .WithTracing(builder =>
                {
                    builder.SetResourceBuilder(resourceBuilder)
                            .AddAspNetCoreInstrumentation()
                            .AddHttpClientInstrumentation()
                            .AddOtlpExporter();
                })
                .WithMetrics(builder =>
                {
                    builder.SetResourceBuilder(resourceBuilder)
                        .AddRuntimeInstrumentation()
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddOtlpExporter();
                })
                .WithLogging(builder =>
                {
                    builder
                        .SetResourceBuilder(resourceBuilder)
                        .AddOtlpExporter();
                });
        }
        public abstract void AddOtlpExporter(OtlpExporterOptions exporterOptions, string signal);
        public static ResourceBuilder ConfigureResourceBuilder()
        {
            var assemblyName = Assembly.GetEntryAssembly().GetName();
            var serviceVersion = assemblyName.Version?.ToString() ?? "unknown";
            var resourceBuilder = ResourceBuilder.CreateDefault();
            return resourceBuilder.AddService(
                    serviceName: assemblyName.Name!,
                    serviceVersion: serviceVersion,
                    serviceInstanceId: Environment.MachineName);
                
        }
    }
}