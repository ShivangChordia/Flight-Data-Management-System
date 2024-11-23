using Microsoft.AspNetCore.SignalR;

namespace Backend_Ground_Terminal
{
    public class TelemetryHub : Hub
    {
        // Method to broadcast data to all connected clients
        public async Task BroadcastTelemetry(string packet)
        {
            await Clients.All.SendAsync("ReceiveTelemetry", packet);
        }
    }
}
