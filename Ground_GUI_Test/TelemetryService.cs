using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace Ground_GUI_Test
{
    public class TelemetryService
    {
        private readonly HubConnection _hubConnection;

        public event Action<string> OnTelemetryReceived;

        public TelemetryService()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("https://localhost:5000/telemetryhub")
                .Build();

            _hubConnection.On<string>("ReceiveTelemetry", (message) =>
            {
                OnTelemetryReceived?.Invoke(message); 
            });
        }

        public async Task StartAsync()
        {
            await _hubConnection.StartAsync();
        }

        public async Task StopAsync()
        {
            await _hubConnection.StopAsync();
        }
    }
}
