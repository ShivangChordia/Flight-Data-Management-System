using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Ground_Terminal_Management_System.Services
{
  

public class TcpMessageReaderService
    {
        private readonly int _port;

        public TcpMessageReaderService(int port)
        {
            _port = port;
        }

        public void Start()
        {
            Console.WriteLine($"Starting TCP Message Reader on port {_port}...");
            TcpListener listener = new TcpListener(IPAddress.Any, _port);
            listener.Start();

            while (true)
            {
                Console.WriteLine("Waiting for client connections...");
                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine("Client connected.");

                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int bytesRead;

                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Received: {message}");

                    // Parse and forward to Database Service
                    var telemetryData = TelemetryParser.Parse(message);
                    if (telemetryData == null)
                    {
                        Console.WriteLine("Invalid telemetry data. Skipping...");
                        continue;
                    }

                    DatabaseService.StoreTelemetryData(telemetryData);

                    // Optionally notify the UI Service
                    //RealTimeUIService.NotifyUI(telemetryData);
                }

                client.Close();
            }
        }
    }

}
