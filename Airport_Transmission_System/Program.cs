using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AircraftTransmissionSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Aircraft Transmission System Started...");

            // File paths for the telemetry data files
            string[] filePaths = { "TelemetryFiles/C-QWWT.txt", "TelemetryFiles/C-GEFC.txt", "TelemetryFiles/C-FGAX.txt" };

            // Start processing files for each aircraft
            foreach (var filePath in filePaths)
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"Error: File not found at {filePath}");
                    continue;
                }
            }

            Console.WriteLine("Transmission system is running...");
        }

    }
}
