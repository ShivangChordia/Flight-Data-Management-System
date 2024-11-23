/*
* FILE : Program.cs
* PROJECT : SENG3020 - Milestone #2 #2
* PROGRAMMER : Shivang Chordia, Keval PAtel, Urvish Motivaras & Jaygiri Goswami
* DATE : 2024-11-22
* DESCRIPTION : This C# program simulates an Aircraft Transmission System that reads telemetry data from files, formats it into structured packets, and transmits the packets to a server over TCP. 
* It processes multiple files concurrently using threads, formats data with sequence numbers and checksums, and handles errors in file reading, data formatting, and network transmission.
* Configuration settings, including file paths, server IP, and port, are loaded from app.config. The program is designed for testing or simulating aircraft telemetry transmission in real time.
*/

using System.Configuration;
using System.Net.Sockets;
using System.Text;

namespace AircraftTransmissionSystem
{
    class Program
    {
        /*
         * FUNCTION: Main
         * DESCRIPTION: The Main function initializes the system, reads configuration settings, validates telemetry files, and starts threads to process and transmit each file's data to a 
                        server concurrently.
         * PARAMETERS: string[] args
         * RETURN: void
         */
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


        /*
         * FUNCTION: ProcessTelemetryFile
         * DESCRIPTION: The ProcessTelemetryFile function reads a telemetry file line by line, formats each line into a packet with a sequence number, and transmits it to a server.
                        It introduces a 1-second delay between transmissions to simulate real-time data flow.
         * PARAMETERS: string filePath -> Path to the telemtry files to get them processed, 
         *             string serverIP -> IP address of the server to which the packets should be transmitted , 
         *             int serverPort -> Port Number of the server to which the packets should be transmitted
         * RETURN: void
         */
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

        /*
         * FUNCTION: FormatPacket
         * DESCRIPTION: The FormatPacket function formats telemetry data into a structured packet. It extracts the aircraft tail number from the file name, parses the telemetry fields, validates 
                        their format, calculates a checksum using altitude, pitch, and bank values, and constructs a packet string with sequence number and telemetry details.
         * PARAMETERS: string filePath -> Path to the telemtry files to get them processed,
         *             int packetSequence -> The sequence number of the packet to ensure the order of packets., 
         *             string telemetryData -> A string containing the telemetry data in CSV format
         * RETURN: void
         */
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
            string weight = fields[4];
            string altitude = fields[5];
            string pitch = fields[6];
            string bank = fields[7];

            // Calculate checksum
            int checksum = CalculateChecksum(altitude, pitch, bank);

            // Construct the packet
            string packet = $"{tailNumber}|{packetSequence}|{timestamp},{x},{y},{z}, {weight}, {altitude},{pitch},{bank}|{checksum}";
            Console.WriteLine($"Formatted packet: {packet}");
            return packet;
        }

        /*
         * FUNCTION: FormatPacket
         * DESCRIPTION: The CalculateChecksum function computes a checksum by parsing the altitude, pitch, and bank values as floats, averaging them, and truncating the result to an integer.
         * PARAMETERS: string altitude ->  The altitude value retrieved from the file as a string,
         *             string pitch  ->  The pitch value retrieved from the file as a string, 
         *             string bank  ->  The bank value retrieved from the file as a string
         * RETURN: int - Returns the checksum, if an error occurs it returns null instead
         */
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


        /*
         * FUNCTION: TransmitPacket
         * DESCRIPTION: The TransmitPacket function sends a formatted packet to a server using TCP. It establishes a connection to the specified server IP and port, converts the packet to a byte 
                        array, and writes it to the network stream. If any errors occur during transmission, they are logged.
         * PARAMETERS:string packet -> Packetized data retrieved from the files to get it transmitted, 
         *             string serverIP -> IP address of the server to which the packets should be transmitted , 
         *             int serverPort -> Port Number of the server to which the packets should be transmitted
         * RETURN: void
         */
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
