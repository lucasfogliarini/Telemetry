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

/// refs:
/// https://docs.dynatrace.com/docs/extend-dynatrace/opentelemetry/walkthroughs/dotnet
/// https://docs.dynatrace.com/docs/extend-dynatrace/opentelemetry/getting-started/otlp-export
/// 
/// See this documentaiton to create a Access Token {env}.live.dynatrace.com/ui/access-tokens/create
/// https://docs.dynatrace.com/docs/shortlink/otel-getstarted-otlpexport#authentication-export-to-activegate

namespace OpenTelemetry
{
    public static class DependencyInjection
    {
        const string otlpEndpoint = "https://gmg83935.live.dynatrace.com/api/v2/otlp";
        const string token = "dt0c01.WWBUR47J2GGTKO62YWVH3MJF.NSVSKYNVFOP5QGA4N7TZ4CYVQ7XBSQFRHXVKVS46WGSGOCVIREPWDS5STE3XUT2G";
        
        public static void AddDynatrace(this IHostApplicationBuilder builder)
        {
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
                                SetDynatraceOtlp(otlpOptions, "v1/traces");
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
                            SetDynatraceOtlp(exporterOptions, "v1/metrics");
                            readerOptions.TemporalityPreference = MetricReaderTemporalityPreference.Delta;
                        });
                });
                builder.Logging.ClearProviders();
                builder.Logging.AddOpenTelemetry(options =>
                {
                    options.SetResourceBuilder(resourceBuilder);
                    options.AddOtlpExporter(otlpOptions => {
                        SetDynatraceOtlp(otlpOptions, "v1/logs");
                    });
                });
        }
        private static void SetDynatraceOtlp(OtlpExporterOptions otlpExporterOptions, string signalPath = "v1/traces")
        {
            otlpExporterOptions.Endpoint = new Uri($"{otlpEndpoint}/{signalPath}");
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