/*
* FILE : TelemetryPrser.cs
* PROJECT : SENG3020 - Milestone #2 
* PROGRAMMER : Shivang Chordia, Keval PAtel, Urvish Motivaras & Jaygiri Goswami
* DATE : 2024-11-22
* DESCRIPTION : This file defines the TelemetryParser class, which is responsible for parsing telemetry messages and extracting data into a structured model (TelemetryDataModel). 
*/


using Backend_Ground_Terminal.Model;

namespace Ground_Terminal_Management_System
{
    public static class TelemetryParser
    {
        /*
       * FUNCTION: Parse()
       * DESCRIPTION: The Parse method, processes and validates telemetry data from a message in string format, then returns a TelemetryDataModel object if the data is valid.
       * PARAMETERS: string message -> This is the raw telemetry message (as a string) received from the client. 
       * RETURN: TelemetryDataModel -> If the checksum is valid, a TelemetryDataModel object is returned with the parse values otherwise if an error occurs during parsing an 
                 empty TelemetryDataModel is returned
       */
        public static TelemetryDataModel Parse(string message)
        {
            try
            {
                // Split the message into parts
                var parts = message.Split('|');
                if (parts.Length != 4) throw new Exception("Invalid message format.");

                var telemetryParts = parts[2].Split(',');
                if (telemetryParts.Length != 8) throw new Exception("Invalid telemetry data format.");

                // Extract data
                string tailNumber = parts[0];

                int sequenceNumber = int.Parse(parts[1]);

                // parts[2] -> telemetry parts
                string timestamp = telemetryParts[0];
                float x = float.Parse(telemetryParts[1]);
                float y = float.Parse(telemetryParts[2]);
                float z = float.Parse(telemetryParts[3]);
                float weight = float.Parse(telemetryParts[4]);
                float altitude = float.Parse(telemetryParts[5]);
                float pitch = float.Parse(telemetryParts[6]);
                float bank = float.Parse(telemetryParts[7]);

                int receivedChecksum = int.Parse(parts[3]);

                // Calculate the checksum
                int calculatedChecksum = CalculateChecksum(altitude, pitch, bank);

                // Validate checksum
                if (calculatedChecksum != receivedChecksum)
                {
                    Console.WriteLine($"Checksum mismatch for TailNumber {tailNumber}: " +
                                      $"Received {receivedChecksum}, Calculated {calculatedChecksum}");
                    return new TelemetryDataModel(); // Return an empty TelemetryDataModel on checksum mismatch                                     
                }

                    // Create and return the telemetry data model
                    return new TelemetryDataModel
                {
                    TailNumber = tailNumber,
                    SequenceNumber = sequenceNumber,
                    Timestamp = DateTime.TryParse(timestamp, out DateTime parsedTimestamp) ? parsedTimestamp : DateTime.MinValue,
                    X = x,
                    Y = y,
                    Z = z,
                    Weight = weight,
                    Altitude = altitude,
                    Pitch = pitch,
                    Bank = bank,
                    Checksum = receivedChecksum
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing message: {ex.Message}");
                return new TelemetryDataModel();  // Return empty TelemetryData on error
            }
        }

        /*
         * FUNCTION: CalculateChecksum()
         * DESCRIPTION: The CalculateChecksum method calculates a checksum based on three telemetry data values: altitude, pitch, and bank. This checksum is used for data validation to ensure the 
                        integrity of the telemetry message.
         * PARAMETERS: float altitude, float pitch, float bank
         * RETURN: Int -> The method returns the calculated checksum as an integer.
         */
        private static int CalculateChecksum(float altitude, float pitch, float bank)
        {
            try
            {
                // Calculate checksum as average of altitude, pitch, and bank
                float checksumFloat = (altitude + pitch + bank) / 3;

                // Convert to integer by truncating
                return (int)Math.Floor(checksumFloat);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating checksum: {ex.Message}");
                throw;
            }
        }
    }

}
