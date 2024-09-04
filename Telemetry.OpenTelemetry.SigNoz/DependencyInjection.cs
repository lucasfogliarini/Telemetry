using Microsoft.Extensions.DependencyInjection;
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
        public static void AddTelemetry(this IServiceCollection services)
        {
            services.AddOpenTelemetry()
                .WithTracing(tracing => tracing
                    .AddAspNetCoreInstrumentation()
                    //.AddOtlpExporter(otlpOptions =>
                    //{
                    //    otlpOptions.Endpoint = new Uri("http://localhost:4317");
                    //})
                    .AddSignOzCloud()
                );
        }
        private static TracerProviderBuilder AddSignOzCloud(this TracerProviderBuilder builder)
        {
            return builder.AddOtlpExporter(otlpOptions =>
            {
                //SigNoz Cloud Endpoint
                otlpOptions.Endpoint = new Uri("https://ingest.us.signoz.cloud:443");

                otlpOptions.Protocol = OtlpExportProtocol.Grpc;

                //SigNoz Cloud account Ingestion key
                string headerKey = "signoz-access-token";
                string headerValue = "844c834d-398f-4416-9ee8-7c1b48c482e9";

                string formattedHeader = $"{headerKey}={headerValue}";
                otlpOptions.Headers = formattedHeader;
            });
        }
        private static OpenTelemetryBuilder AddOpenTelemetry(this IServiceCollection services)
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName();
            var serviceVersion = assemblyName.Version?.ToString() ?? "unknown";
            return services.AddOpenTelemetry()
                .ConfigureResource(r => r
                .AddService(
                    serviceName: assemblyName.Name!,
                    serviceVersion: serviceVersion,
                    serviceInstanceId: Environment.MachineName));
        }
    }
}