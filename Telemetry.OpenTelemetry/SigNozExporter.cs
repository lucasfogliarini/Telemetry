using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter;

namespace OpenTelemetry
{
    public class SigNozExporter(IHostApplicationBuilder builder) : OtlpExporter(builder)
    {
        const string tokenInstructions = "Não há um token para adicionar o SigNoz, veja essa documentação para obter um token válido: https://{your_env}.us.signoz.cloud/get-started/application-monitoring";
        const string endpointConfigKey = "SigNozOtlp:endpoint";
        const string tokenConfigKey = "SigNozOtlp:token";

        public override void AddOtlpExporter(OtlpExporterOptions otlpExporterOptions, string signal)
        {
            var endpoint = builder.Configuration[endpointConfigKey]!;
            var token = builder.Configuration[tokenConfigKey];
            if (string.IsNullOrWhiteSpace(token))
                throw new InvalidOperationException(tokenInstructions);

            otlpExporterOptions.Endpoint = new Uri(endpoint);
            otlpExporterOptions.Protocol = OtlpExportProtocol.Grpc;

            string formattedHeader = $"signoz-access-token={token}";
            otlpExporterOptions.Headers = formattedHeader;
        }
    }
}