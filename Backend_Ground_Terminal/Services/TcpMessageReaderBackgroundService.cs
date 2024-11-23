/*
* FILE : TcpMessageReaderBackgroundService.cs
* PROJECT : SENG3020 - Milestone #2 
* PROGRAMMER : Shivang Chordia, Keval PAtel, Urvish Motivaras & Jaygiri Goswami
* DATE : 2024-11-22
* DESCRIPTION : This file defines the TelemetryController class, an API controller for managing telemetry data in the Ground Terminal Management System. 
*/


namespace Ground_Terminal_Management_System.Services
{
    public class TcpMessageReaderBackgroundService : BackgroundService
    {
        private readonly TcpMessageReaderService _tcpMessageReaderService;

        /*
       * CONSTRUCTOR: TcpMessageReaderBackgroundService()
       * DESCRIPTION: Initializes the TcpMessageReaderBackgroundService class by injecting a dependency on the TcpMessageReaderService.
       * PARAMETERS: TcpMessageReaderService tcpMessageReaderService
       * RETURN: n/a
       */
        public TcpMessageReaderBackgroundService(TcpMessageReaderService tcpMessageReaderService)
        {
            _tcpMessageReaderService = tcpMessageReaderService;
        }

        /*
       * FUNCTION: ExecuteAsync()
       * DESCRIPTION: Implements the core functionality of the background service by starting the TcpMessageReaderService.
       * PARAMETERS: CancellationToken stoppingToken -> This token is used to handle cancellation signals, allowing the background service to shut down gracefully if needed.
       * RETURN: n/a
       */
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _tcpMessageReaderService.StartAsync(stoppingToken);
        }
    }
}
