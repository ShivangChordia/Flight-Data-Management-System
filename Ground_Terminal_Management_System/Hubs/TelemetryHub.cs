using Ground_Terminal_Management_System.Model;
using Microsoft.AspNetCore.SignalR;

namespace Ground_Terminal_Management_System.Hubs
{
  

    public class TelemetryHub : Hub
    {
        // This method is used to send telemetry data to clients
        public async Task SendTelemetryData(TelemetryDataModel data)
        {
            Console.WriteLine($"Sending data: {data.TailNumber}, {data.Altitude}");
            await Clients.All.SendAsync("ReceiveTelemetryData", data);
        }
    }

}
