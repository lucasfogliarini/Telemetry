using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using System.Reflection;

namespace OpenTelemetry
{
    public static class DependencyInjection
    {
        public static void AddAzureMonitorExporter(this IHostApplicationBuilder builder)
        {
            builder.Services.ConfigureOpenTelemetry().UseAzureMonitor();
        }
        private static OpenTelemetryBuilder ConfigureOpenTelemetry(this IServiceCollection services)
        {
            var assemblyName = Assembly.GetEntryAssembly().GetName();
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