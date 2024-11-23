using System.Net.Sockets;
using System.Net;
using System.Text;
using Backend_Ground_Terminal.Model;
using Backend_Ground_Terminal;
using Microsoft.AspNetCore.SignalR;

namespace Ground_Terminal_Management_System.Services
{
    public class TcpMessageReaderService
    {
        private readonly int _port;
        private TcpListener? _listener;
        private CancellationTokenSource? _cancellationTokenSource;
        private readonly DatabaseService _databaseService;
        private readonly IHubContext<TelemetryHub> _hubContext; // SignalR Hub Context

        public TcpMessageReaderService(int port, DatabaseService databaseService, IHubContext<TelemetryHub> hubContext)
        {
            _port = port;
            _databaseService = databaseService;
            _hubContext = hubContext;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine($"Starting TCP Message Reader on port {_port}...");
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            try
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    Console.WriteLine("Waiting for client connections...");
                    TcpClient client = await _listener.AcceptTcpClientAsync(_cancellationTokenSource.Token);

                    // Handle the client connection in a separate task.
                    _ = Task.Run(() => HandleClientAsync(client, _cancellationTokenSource.Token));
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("TCP Message Reader service is stopping...");
            }
            finally
            {
                _listener.Stop();
                Console.WriteLine("TCP Message Reader stopped.");
            }
        }

        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
        {
            Console.WriteLine("Client connected.");
            using NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {

                    int bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);

                    if (bytesRead == 0)
                    {
                        Console.WriteLine("Client disconnected.");
                        break;
                    }

                    string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Received: {message}");

                    // Parse and forward to Database Service
                    var telemetryData = TelemetryParser.Parse(message);
                    if (telemetryData == null)
                    {
                        Console.WriteLine("Invalid telemetry data. Skipping...");
                        continue;
                    }

                    // Process Telemetry data and forward it to Blazor page
                    ProcessTelemetryDataAsync(telemetryData, message);

                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client: {ex.Message}");
            }
            finally
            {
                client.Close();
            }
        }

        private async Task ProcessTelemetryDataAsync(TelemetryDataModel telemetryData, String message)
        {
            try
            {
                _databaseService.StoreTelemetryData(telemetryData); // Store in Database
                
                Console.WriteLine("Telemetry data successfully stored!");

                // Broadcast telemetry data to SignalR clients
                await _hubContext.Clients.All.SendAsync("ReceiveTelemetry", message);
                Console.WriteLine("Telemetry data broadcasted to SignalR clients.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error storing telemetry data: {ex.Message}");
            }
        }
    }
}
