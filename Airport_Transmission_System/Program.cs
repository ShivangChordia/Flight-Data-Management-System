/*
 * Header comment
 */

using System;
using System.Configuration;
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

            // Read file paths from app.config
            string[] filePaths = new string[]
            {
                ConfigurationManager.AppSettings["FilePath1"],
                ConfigurationManager.AppSettings["FilePath2"],
                ConfigurationManager.AppSettings["FilePath3"]
            };

            // Read IP and Port from app.config
            string serverIP = ConfigurationManager.AppSettings["ServerIP"];
            int serverPort = int.Parse(ConfigurationManager.AppSettings["ServerPort"]);

            // Start processing files for each aircraft
            foreach (var filePath in filePaths)
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"Error: File not found at {filePath}");
                    continue;
                }

                // Start a new thread to process each file
                Thread thread = new Thread(() => ProcessTelemetryFile(filePath, serverIP, serverPort));
                thread.Start();
            }

            Console.WriteLine("Transmission system is running...");
        }

        static void ProcessTelemetryFile(string filePath, string serverIP, int serverPort)
        {
            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;
                    int packetSequence = 1;

                    while ((line = reader.ReadLine()) != null)
                    {
                        // Format the packet with a sequence number
                        string packet = FormatPacket(filePath, packetSequence, line);

                        // Transmit the packet
                        TransmitPacket(packet, serverIP, serverPort);

                        // Increment sequence number
                        packetSequence++;

                        // Wait for 1 second before processing the next line
                        Thread.Sleep(1000);
                    }
                }

                Console.WriteLine($"Finished processing file: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing file {filePath}: {ex.Message}");
            }
        }

        static string FormatPacket(string filePath, int packetSequence, string telemetryData)
        {
            // Extract the Aircraft Tail # from the filename (e.g., "Aircraft1.txt" -> "Aircraft1")
            string tailNumber = Path.GetFileNameWithoutExtension(filePath);

            // Parse the telemetry data fields
            var fields = telemetryData.Split(',');
            if (fields.Length < 8)
            {
                throw new Exception("Invalid telemetry data format.");
            }

            string timestamp = fields[0];
            string x = fields[1];
            string y = fields[2];
            string z = fields[3];
            string altitude = fields[5];
            string pitch = fields[6];
            string bank = fields[7];

            // Calculate checksum
            int checksum = CalculateChecksum(altitude, pitch, bank);

            // Construct the packet
            string packet = $"{tailNumber}|{packetSequence}|{timestamp},{x},{y},{z},{altitude},{pitch},{bank}|{checksum}";
            Console.WriteLine($"Formatted packet: {packet}");
            return packet;
        }

        static int CalculateChecksum(string altitude, string pitch, string bank)
        {
            try
            {
                // Parse the float values
                float alt = float.Parse(altitude);
                float pit = float.Parse(pitch);
                float ban = float.Parse(bank);

                // Perform checksum calculation
                float checksumFloat = (alt + pit + ban) / 3;

                // Convert to integer by truncating
                int checksum = (int)Math.Floor(checksumFloat);

                return checksum;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating checksum: {ex.Message}");
                return 0; // Default to 0 on error
            }
        }

        static void TransmitPacket(string packet, string serverIP, int serverPort)
        {
            try
            {
                Console.WriteLine($"Transmitting packet: {packet}");

                // Establish a connection using the IP and Port from app.config
                using (TcpClient client = new TcpClient(serverIP, serverPort))
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] data = Encoding.ASCII.GetBytes(packet);
                    stream.Write(data, 0, data.Length);
                }

                Console.WriteLine("Packet transmitted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error transmitting packet: {ex.Message}");
            }
        }
    }
}
