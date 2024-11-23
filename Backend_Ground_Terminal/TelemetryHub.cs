/*
* FILE : TelemetryHub.cs
* PROJECT : SENG3020 - Milestone #2 
* PROGRAMMER : Shivang Chordia, Keval PAtel, Urvish Motivaras & Jaygiri Goswami
* DATE : 2024-11-22
* DESCRIPTION : This file defines the TelemetryHub class, which is a SignalR hub used for real-time communication in the Backend Ground Terminal system. 
*/

using Microsoft.AspNetCore.SignalR;

namespace Backend_Ground_Terminal
{
    public class TelemetryHub : Hub
    {
        /*
       * FUNCTION: BroadcastTelemetry()
       * DESCRIPTION: Method to broadcast data to all connected clients
       * PARAMETERS: string packet -> This parameter represents the telemetry data packet that will be broadcasted to all clients. 
       * RETURN: Task 
       */
        // 
        public async Task BroadcastTelemetry(string packet)
        {
            await Clients.All.SendAsync("ReceiveTelemetry", packet);
        }
    }
}
