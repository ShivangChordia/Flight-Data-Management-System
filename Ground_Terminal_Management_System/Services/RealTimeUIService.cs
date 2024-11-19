using Ground_Terminal_Management_System.Model;

namespace Ground_Terminal_Management_System.Services
{
    public static class RealTimeUIService
    {
        public static event Action<TelemetryDataModel> OnTelemetryDataReceived;

        public static void NotifyUI(TelemetryDataModel data)
        {
            Console.WriteLine($"Notifying UI with telemetry data: {data.TailNumber}");
            OnTelemetryDataReceived?.Invoke(data);
        }
    }

}
