/*
* FILE : TelemetryHub.cs
* PROJECT : SENG3020 - Milestone #2 
* PROGRAMMER : Shivang Chordia, Keval PAtel, Urvish Motivaras & Jaygiri Goswami
* DATE : 2024-11-22
* DESCRIPTION : This file defines the TelemetryHub class, which is a SignalR hub used for real-time communication in the Backend Ground Terminal system. 
*/

using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using Backend_Ground_Terminal.Model;

namespace Backend_Ground_Terminal.Hubs
{
    public class TelemetryHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> _connectedClients = new();

        // Expose the connected clients as a read-only property
        public static int ConnectedClientCount => _connectedClients.Count;
        public override Task OnConnectedAsync()
        {
            _connectedClients.TryAdd(Context.ConnectionId, "Connected");
            Console.WriteLine($"Client connected: {Context.ConnectionId}. Active clients: {_connectedClients.Count}");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _connectedClients.TryRemove(Context.ConnectionId, out _);
            Console.WriteLine($"Client disconnected: {Context.ConnectionId}. Active clients: {_connectedClients.Count}");
            if (exception != null)
            {
                Console.WriteLine($"Disconnection error: {exception.Message}");
            }
            return base.OnDisconnectedAsync(exception);
        }

        /*
       * FUNCTION: BroadcastTelemetry()
       * DESCRIPTION: Method to broadcast data to all connected clients
       * PARAMETERS: string packet -> This parameter represents the telemetry data packet that will be broadcasted to all clients. 
       * RETURN: Task 
       */
        // 
        public async Task BroadcastTelemetry(TelemetryDataModel telemetryData)
        {
            await Clients.All.SendAsync("ReceiveTelemetry", telemetryData);
        }


    }
}
