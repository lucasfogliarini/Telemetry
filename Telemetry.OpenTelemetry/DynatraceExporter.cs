using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter;

namespace OpenTelemetry
{
    public class DynatraceExporter(IHostApplicationBuilder builder) : OtlpExporter(builder)
    {
        const string tokenInstructions = "Não há um token para adicionar o Dynatrace, veja essa documentação para obter um token válido: https://docs.dynatrace.com/docs/shortlink/otel-getstarted-otlpexport#authentication-export-to-activegate";
        const string endpointConfigKey = "DynatraceOtlp:endpoint";
        const string tokenConfigKey = "DynatraceOtlp:token";

        public override void AddOtlpExporter(OtlpExporterOptions otlpExporterOptions, string signal)
        {
            var endpoint = builder.Configuration[endpointConfigKey]!;
            var token = builder.Configuration[tokenConfigKey];
            if (string.IsNullOrWhiteSpace(token))
                throw new InvalidOperationException(tokenInstructions);

            otlpExporterOptions.Endpoint = new Uri($"{endpoint}/v1/{signal}");
            otlpExporterOptions.Protocol = OtlpExportProtocol.HttpProtobuf;
            otlpExporterOptions.Headers = $"Authorization=Api-Token {token}";
        }
    }
}