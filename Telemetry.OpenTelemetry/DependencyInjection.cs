using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

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
            var resourceBuilder = OtlpExporter.ConfigureResourceBuilder();
            builder.Services
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
                        .AddHttpClientInstrumentation()
                        .AddAspNetCoreInstrumentation()
                        .AddOtlpExporter();
                })
                .WithLogging(builder =>
                {
                    builder.SetResourceBuilder(resourceBuilder)
                        .AddOtlpExporter();
                });

            return builder;
        }

        public static IHostApplicationBuilder UseDynatraceExporter(this IHostApplicationBuilder builder)
        {
            var dynatraceExporter = new DynatraceExporter(builder);
            dynatraceExporter.Build();
            return builder;
        }

        public static IHostApplicationBuilder UseSigNozExporter(this IHostApplicationBuilder builder)
        {
            var sigNozExporter = new SigNozExporter(builder);
            sigNozExporter.Build();
            return builder;
        }
    }
}