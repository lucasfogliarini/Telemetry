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
        const string tokenInstructions = "Não há um token para adicionar o SigNoz, veja essa documentação para obter um token válido: https://{your_env}.us.signoz.cloud/get-started/application-monitoring";
        const string endpointConfigKey = "SigNozOtlp:endpoint";
        const string tokenConfigKey = "SigNozOtlp:token";
        public static void AddSigNoz(this IHostApplicationBuilder builder)
        {
            var endpoint = builder.Configuration[endpointConfigKey]!;
            var token = builder.Configuration[tokenConfigKey];
            if (string.IsNullOrWhiteSpace(token))
                throw new InvalidOperationException(tokenInstructions);
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
                            SetSigNozOtlp(exporterOptions, endpoint, token);
                        });
                })
                .WithTracing(builder =>
                {
                    builder
                        .SetResourceBuilder(resourceBuilder)
                        //.AddHttpClientInstrumentation()
                        .AddAspNetCoreInstrumentation()
                        .AddOtlpExporter(exporterOptions =>
                        {
                            SetSigNozOtlp(exporterOptions, endpoint, token);
                        });
                })
                .WithMetrics(builder =>
                {
                    builder.SetResourceBuilder(resourceBuilder)
                        .AddAspNetCoreInstrumentation()
                        .AddOtlpExporter((OtlpExporterOptions exporterOptions, MetricReaderOptions readerOptions) =>
                        {
                            SetSigNozOtlp(exporterOptions, endpoint, token);
                            readerOptions.TemporalityPreference = MetricReaderTemporalityPreference.Delta;
                        });
                });
        }
        private static void SetSigNozOtlp(OtlpExporterOptions otlpExporterOptions, string endpoint, string token)
        {
            otlpExporterOptions.Endpoint = new Uri(endpoint);
            otlpExporterOptions.Protocol = OtlpExportProtocol.Grpc;

            string headerKey = "signoz-access-token";
            string headerValue = token;

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