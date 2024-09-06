using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;
using System.Runtime.Serialization;

namespace OpenTelemetry
{
    public static class DependencyInjection
    {
        const string tokenInstructions = "Não há um token para adicionar o Dynatrace, veja essa documentação para obter um token válido: https://docs.dynatrace.com/docs/shortlink/otel-getstarted-otlpexport#authentication-export-to-activegate";
        const string endpointConfigKey = "DynatraceOtlp:endpoint";
        const string tokenConfigKey = "DynatraceOtlp:token";

        public static void AddDynatrace(this IHostApplicationBuilder builder)
        {
            var endpoint = builder.Configuration[endpointConfigKey]!;
            var token = builder.Configuration[tokenConfigKey];
            if(string.IsNullOrWhiteSpace(token))
                throw new InvalidOperationException(tokenInstructions);

            var resourceBuilder = ConfigureResourceBuilder();
            builder
                .Services
                .AddOpenTelemetry()
                .WithTracing(builder =>
                {
                    builder.SetResourceBuilder(resourceBuilder)
                            //.AddHttpClientInstrumentation()
                            .AddAspNetCoreInstrumentation()
                            .AddOtlpExporter(otlpOptions =>
                            {
                                SetDynatraceOtlp(otlpOptions, endpoint, token, "v1/traces");
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
                            SetDynatraceOtlp(exporterOptions, endpoint, token, "v1/metrics");
                            readerOptions.TemporalityPreference = MetricReaderTemporalityPreference.Delta;
                        });
                });
                builder.Logging.ClearProviders();
                builder.Logging.AddOpenTelemetry(options =>
                {
                    options.SetResourceBuilder(resourceBuilder);
                    options.AddOtlpExporter(otlpOptions => {
                        SetDynatraceOtlp(otlpOptions, endpoint, token, "v1/logs");
                    });
                });
        }
        private static void SetDynatraceOtlp(OtlpExporterOptions otlpExporterOptions, string endpoint, string token, string signalPath = "v1/traces")
        {
            otlpExporterOptions.Endpoint = new Uri($"{endpoint}/{signalPath}");
            otlpExporterOptions.Protocol = OtlpExportProtocol.HttpProtobuf;
            otlpExporterOptions.Headers = $"Authorization=Api-Token {token}";
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