using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;

namespace OpenTelemetry
{
    public static class DependencyInjection
    {
        /// <summary>
        /// Use OtlpExporter with env vars
        ///https://github.com/open-telemetry/opentelemetry-dotnet/tree/main/src/OpenTelemetry.Exporter.OpenTelemetryProtocol#enable-otlp-exporter-for-all-signals
        /// </summary>
        public static IHostApplicationBuilder UseOtlpExporter(this IHostApplicationBuilder builder)
        {
            builder.Services
                .AddOpenTelemetry()
                .ConfigureResource(ConfigureTelemetryResource)
                .WithTracing(builder =>
                {
                    builder
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddOtlpExporter();
                })
                .WithMetrics(builder =>
                {
                    builder
                        .AddRuntimeInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddAspNetCoreInstrumentation()
                        .AddOtlpExporter();
                })
                .WithLogging(builder =>
                {
                    builder.AddOtlpExporter();
                });

            return builder;
        }

        private static void ConfigureTelemetryResource(ResourceBuilder resourceBuilder)
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName();
            var serviceVersion = assemblyName.Version?.ToString() ?? "unknown";
            resourceBuilder.AddService(
                serviceName: assemblyName.Name!,
                serviceVersion: serviceVersion,
                serviceNamespace: assemblyName.Name!,
                serviceInstanceId: Environment.MachineName);
        }
    }
}