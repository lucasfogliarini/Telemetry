using Microsoft.Extensions.Hosting;

namespace OpenTelemetry
{
    public static class DependencyInjection
    {
        public static IHostApplicationBuilder AddDynatraceExporter(this IHostApplicationBuilder builder)
        {
            var dynatraceExporter = new DynatraceExporter(builder);
            dynatraceExporter.Build();
            return builder;
        }

        public static IHostApplicationBuilder AddSigNozExporter(this IHostApplicationBuilder builder)
        {
            var sigNozExporter = new SigNozExporter(builder);
            sigNozExporter.Build();
            return builder;
        }
    }
}