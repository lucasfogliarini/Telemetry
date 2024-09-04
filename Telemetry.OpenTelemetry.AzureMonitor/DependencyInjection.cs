using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;

namespace OpenTelemetry
{
    public static class DependencyInjection
    {
        public static void AddAzureMonitor(this IServiceCollection services)
        {
            services.ConfigureOpenTelemetry().UseAzureMonitor();
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