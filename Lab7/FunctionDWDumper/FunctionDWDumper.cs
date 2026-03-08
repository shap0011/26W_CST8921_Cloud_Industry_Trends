using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FunctionDWDumper
{
    public class FunctionDWDumper
    {
        private readonly ILogger<FunctionDWDumper> _logger;

        public FunctionDWDumper(ILogger<FunctionDWDumper> logger)
        {
            _logger = logger;
        }

        [Function("FunctionDWDumper")]
        public async Task Run(
            [EventHubTrigger("turbine-telemetry", Connection = "EventHubConnection", IsBatched = false)] string eventData,
            FunctionContext context)
        {
            _logger.LogInformation($"Event received: {eventData}");

            var telemetry = JsonSerializer.Deserialize<TelemetryPayload>(
                eventData,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (telemetry == null || string.IsNullOrWhiteSpace(telemetry.DeviceId) || string.IsNullOrWhiteSpace(telemetry.Timestamp))
            {
                _logger.LogError("Telemetry payload is missing required fields.");
                return;
            }

            string status = (telemetry.WindSpeed > 15 && telemetry.GeneratedPower < 5)
                ? "URGENT"
                : "HEALTHY";

            string? connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                _logger.LogError("AzureWebJobsStorage is not configured.");
                return;
            }

            var tableClient = new TableClient(connectionString, "TurbineMetrics");
            await tableClient.CreateIfNotExistsAsync();

            var entity = new TableEntity(telemetry.DeviceId, telemetry.Timestamp)
            {
                { "WindSpeed", telemetry.WindSpeed },
                { "GeneratedPower", telemetry.GeneratedPower },
                { "TurbineSpeed", telemetry.TurbineSpeed },
                { "Status", status }
            };

            await tableClient.AddEntityAsync(entity);
            _logger.LogInformation($"Saved: DeviceId={telemetry.DeviceId}, Status={status}");
        }
    }
}