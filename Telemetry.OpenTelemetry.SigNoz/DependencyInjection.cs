using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;

namespace OpenTelemetry
{
    public static class DependencyInjection
    {
        const string otlpEndpoint = "https://ingest.us.signoz.cloud:443";
        const string signoz_key = "844c834d-398f-4416-9ee8-7c1b48c482e9";
        public static void AddSigNoz(this IHostApplicationBuilder builder)
        {
            var resourceBuilder = ConfigureResourceBuilder();
            builder
                .Services
                .AddOpenTelemetry()
                .WithLogging(builder =>
                {
                    builder
                        .SetResourceBuilder(resourceBuilder)
                        .AddOtlpExporter((OtlpExporterOptions exporterOptions) =>
                        {
                            SetSigNozOtlp(exporterOptions);
                        });
                })
                .WithTracing(builder =>
                {
                    builder
                        .SetResourceBuilder(resourceBuilder)
                        //.AddHttpClientInstrumentation()
                        .AddAspNetCoreInstrumentation()
                        .AddOtlpExporter(otlpOptions =>
                        {
                            SetSigNozOtlp(otlpOptions);
                        });
                })
                .WithMetrics(builder =>
                {
                    builder.SetResourceBuilder(resourceBuilder)
                        .AddAspNetCoreInstrumentation()
                        .AddOtlpExporter((OtlpExporterOptions exporterOptions, MetricReaderOptions readerOptions) =>
                        {
                            SetSigNozOtlp(exporterOptions);
                            readerOptions.TemporalityPreference = MetricReaderTemporalityPreference.Delta;
                        });
                });
        }
        private static void SetSigNozOtlp(OtlpExporterOptions otlpExporterOptions)
        {
            otlpExporterOptions.Endpoint = new Uri($"{otlpEndpoint}");
            otlpExporterOptions.Protocol = OtlpExportProtocol.Grpc;

            //SigNoz Cloud account Ingestion key
            string headerKey = "signoz-access-token";
            string headerValue = signoz_key;

            string formattedHeader = $"{headerKey}={headerValue}";
            otlpExporterOptions.Headers = formattedHeader;
        }
        private static ResourceBuilder ConfigureResourceBuilder()
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