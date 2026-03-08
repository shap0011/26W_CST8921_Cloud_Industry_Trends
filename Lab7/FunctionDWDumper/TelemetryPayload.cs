namespace FunctionDWDumper
{
    public class TelemetryPayload
    {
        public string? DeviceId { get; set; }
        public string? Timestamp { get; set; }
        public double WindSpeed { get; set; }
        public double GeneratedPower { get; set; }
        public double TurbineSpeed { get; set; }
    }
}