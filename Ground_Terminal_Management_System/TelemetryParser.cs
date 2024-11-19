using Ground_Terminal_Management_System.Model;

namespace Ground_Terminal_Management_System
{
    public static class TelemetryParser
    {
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
                    return null; // Return null if checksum does not match
                }

                // Create and return the telemetry data model
                return new TelemetryDataModel
                {
                    TailNumber = tailNumber,
                    SequenceNumber = sequenceNumber,
                    Timestamp = timestamp,
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
                return null; // Return null on parsing error
            }
        }

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
