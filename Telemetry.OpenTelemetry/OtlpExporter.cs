using Microsoft.Extensions.Configuration;
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
                            //.AddHttpClientInstrumentation()
                            .AddAspNetCoreInstrumentation()
                            .AddOtlpExporter(exporterOptions =>
                            {
                                AddOtlpExporter(exporterOptions, "traces");
                            });
                })
                .WithMetrics(builder =>
                {
                    builder.SetResourceBuilder(resourceBuilder)
                        //.AddRuntimeInstrumentation()
                        //.AddHttpClientInstrumentation()
                        .AddAspNetCoreInstrumentation()
                        .AddOtlpExporter((OtlpExporterOptions exporterOptions, MetricReaderOptions readerOptions) =>
                        {
                            AddOtlpExporter(exporterOptions, "metrics");
                            readerOptions.TemporalityPreference = MetricReaderTemporalityPreference.Delta;
                        });
                })
                .WithLogging(builder =>
                {
                    builder
                        .SetResourceBuilder(resourceBuilder)
                        .AddOtlpExporter((OtlpExporterOptions exporterOptions) =>
                        {
                            AddOtlpExporter(exporterOptions, "logs");
                        });
                });
        }
        public abstract void AddOtlpExporter(OtlpExporterOptions exporterOptions, string signal);
        private ResourceBuilder ConfigureResourceBuilder()
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