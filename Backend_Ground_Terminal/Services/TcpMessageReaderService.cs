/*
* FILE : TcpMessageReaderService.cs
* PROJECT : SENG3020 - Milestone #2 
* PROGRAMMER : Shivang Chordia, Keval PAtel, Urvish Motivaras & Jaygiri Goswami
* DATE : 2024-11-22
* DESCRIPTION : This file handles incoming telemetry data over a TCP connection, processes it, and integrates it with the database and real-time client interfaces.
*/


using System.Net.Sockets;
using System.Net;
using System.Text;
using SharedModels;
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
        private readonly SemaphoreSlim _semaphore = new(1, 1); // For thread-safe database operations


        /*
       * FUNCTION: TcpMessageReaderService()
       * DESCRIPTION: Listens for incoming telemetry data over a TCP connection. Processes and stores the telemetry data in a database. Broadcasts the data in real-time to clients using SignalR.
       * PARAMETERS: int port - TCP port to listen for incoming client connections
       *             DatabaseService databaseService - Used to store the processed telemtry data.
       *             IHubContext<TelemetryHub> hubContext - Provides the context to broadcast data to SignalR clients.
       * RETURN: n/a
       */
        public TcpMessageReaderService(int port, DatabaseService databaseService, IHubContext<TelemetryHub> hubContext)
        {
            _port = port;
            _databaseService = databaseService;
            _hubContext = hubContext;
        }

        /*
         * FUNCTION: StartAsync()
         * DESCRIPTION: Starts the TCP listener on the specified port and listens for client connections.
         * PARAMETERS: CancellationToken cancellationToken -> This token allows the caller to cancel the execution of the StartAsync method. It is used to gracefully stop the listener when needed.
         * RETURN: Task (Asynchronous Method)
         */
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

                    // Handle each client connection in a separate task
                    _ = Task.Run(() => HandleClientAsync(client, _cancellationTokenSource.Token));
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("TCP Message Reader service is stopping...");
            }
            finally
            {
                _listener?.Stop();
                Console.WriteLine("TCP Message Reader stopped.");
            }
        }


        /*
        * FUNCTION: stop()
        * DESCRIPTION: Stops the TCP message reader service by canceling ongoing tasks and closing the listener.
        * PARAMETERS: none
        * RETURN: void
        */
        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
        }

        /*
        * FUNCTION: HandleClientAsync()
        * DESCRIPTION: Handles communication with a single client, reading messages and processing them.
        * PARAMETERS: TcpClient client -> Represents the client connection that has been established over TCP. , 
        *             CancellationToken cancellationToken -> This token is used to monitor for cancellation requests. It allows the method to respond to cancellation signals, such as when the server 
        *             needs to stop processing or the operation is no longer needed. 
        * RETURN: Task (Asynchronous Method) 
        */
        private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
        {
            var clientEndpoint = client.Client.RemoteEndPoint?.ToString() ?? "Unknown";
            Console.WriteLine($"[TCP] Telemetry transmission system connected: {clientEndpoint}");

            using NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    int bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);

                    if (bytesRead == 0)
                    {
                        Console.WriteLine($"[TCP] Telemetry transmission system disconnected: {clientEndpoint}");
                        break;
                    }

                    string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"[TCP] Received telemetry: {message}");

                    // Parse and process the telemetry data
                    var telemetryData = TelemetryParser.Parse(message);
                    if (telemetryData == null)
                    {
                        Console.WriteLine("[TCP] Invalid telemetry data. Skipping...");
                        continue;
                    }

                    // Safely process telemetry data
                    await ProcessTelemetryDataAsync(telemetryData, message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TCP] Error handling telemetry connection ({clientEndpoint}): {ex.Message}");
            }
            finally
            {
                client.Close();
            }
        }


        /*
        * FUNCTION: ProcessTelemetryDataAsync()
        * DESCRIPTION: Processes the telemetry data by storing it in the database and broadcasting it to SignalR clients.
        * PARAMETERS: TelemetryDataModel telemetryData ->  This is the model containing the parsed telemetry data (e.g., tail number, sequence number, altitude, pitch, bank, etc.) that was received 
        *                                                  and parsed from the client message. 
        *             String message ->  This is the raw telemetry message (as a string) received from the client. 
        * RETURN: n/a
        */
        private async Task ProcessTelemetryDataAsync(TelemetryDataModel telemetryData, string message)
        {
            await _semaphore.WaitAsync(); // Ensure thread-safe access to the database
            try
            {
                // Store telemetry data in the database
                await _databaseService.StoreTelemetryDataAsync(telemetryData);
                Console.WriteLine("Telemetry data successfully stored!");

                // Broadcast only if there are active clients
                var activeClients = TelemetryHub.ConnectedClientCount; // Access from TelemetryHub
                if (activeClients > 0)
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveTelemetry", telemetryData);
                    Console.WriteLine($"Telemetry data broadcasted to {activeClients} SignalR clients.");
                }
                else
                {
                    Console.WriteLine("No active SignalR clients. Skipping broadcast.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing telemetry data: {ex.Message}");
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
