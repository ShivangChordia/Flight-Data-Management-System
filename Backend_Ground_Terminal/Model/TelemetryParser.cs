/*
* FILE : TelemetryPrser.cs
* PROJECT : SENG3020 - Milestone #2 
* PROGRAMMER : Shivang Chordia, Keval PAtel, Urvish Motivaras & Jaygiri Goswami
* DATE : 2024-11-22
* DESCRIPTION : This file defines the TelemetryParser class, which is responsible for parsing telemetry messages and extracting data into a structured model (TelemetryDataModel). 
*/

namespace Backend_Ground_Terminal.Model
{
    public static class TelemetryParser
    {
        /*
        * FUNCTION: Parse()
        * DESCRIPTION: Parses raw telemetry data into a TelemetryDataModel object after validation. Returns null if parsing or validation fails.
        * PARAMETERS: string message -> Raw telemetry message.
        * RETURN: TelemetryDataModel? -> Parsed telemetry data or null if parsing fails.
        */
        public static TelemetryDataModel? Parse(string message)
        {
            try
            {
                // Split the message into parts
                var parts = message.Split('|');
                if (parts.Length != 4)
                {
                    Console.WriteLine("Invalid message format.");
                    return null;
                }

                // Validate telemetry data structure
                var telemetryParts = parts[2].Split(',');
                if (telemetryParts.Length != 8)
                {
                    Console.WriteLine("Invalid telemetry data format.");
                    return null;
                }

                // Extract and validate data
                if (!int.TryParse(parts[1], out int sequenceNumber))
                {
                    Console.WriteLine("Invalid sequence number.");
                    return null;
                }

                if (!int.TryParse(parts[3], out int receivedChecksum))
                {
                    Console.WriteLine("Invalid checksum format.");
                    return null;
                }

                string tailNumber = parts[0];
                string timestampString = telemetryParts[0];
                float x = TryParseFloat(telemetryParts[1], "X");
                float y = TryParseFloat(telemetryParts[2], "Y");
                float z = TryParseFloat(telemetryParts[3], "Z");
                float weight = TryParseFloat(telemetryParts[4], "Weight");
                float altitude = TryParseFloat(telemetryParts[5], "Altitude");
                float pitch = TryParseFloat(telemetryParts[6], "Pitch");
                float bank = TryParseFloat(telemetryParts[7], "Bank");

                // Validate checksum
                int calculatedChecksum = CalculateChecksum(altitude, pitch, bank);
                if (calculatedChecksum != receivedChecksum)
                {
                    Console.WriteLine($"Checksum mismatch for TailNumber {tailNumber}: " +
                                      $"Received {receivedChecksum}, Calculated {calculatedChecksum}");
                    return null;
                }



                // Return the structured telemetry model
                return new TelemetryDataModel
                {
                    TailNumber = tailNumber,
                    SequenceNumber = sequenceNumber,
                    Timestamp = timestampString,
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

        /*
         * FUNCTION: TryParseFloat()
         * DESCRIPTION: Helper method to parse a float value and log errors if parsing fails.
         * PARAMETERS: string value -> Value to parse.
         *             string fieldName -> Field name for error logging.
         * RETURN: float -> Parsed float value or 0.0f if parsing fails.
         */
        private static float TryParseFloat(string value, string fieldName)
        {
            if (!float.TryParse(value, out float result))
            {
                Console.WriteLine($"Invalid value for {fieldName}: {value}. Defaulting to 0.");
                return 0.0f;
            }
            return result;
        }

        /*
         * FUNCTION: CalculateChecksum()
         * DESCRIPTION: Calculates a checksum based on altitude, pitch, and bank.
         * PARAMETERS: float altitude, float pitch, float bank
         * RETURN: int -> Calculated checksum.
         */
        private static int CalculateChecksum(float altitude, float pitch, float bank)
        {
            try
            {
                // Calculate checksum as the average of altitude, pitch, and bank
                float checksumFloat = (altitude + pitch + bank) / 3;
                return (int)Math.Floor(checksumFloat); // Truncate to integer
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating checksum: {ex.Message}");
                throw;
            }
        }

        /*
 * FUNCTION: TryParseTimestamp()
 * DESCRIPTION: Parses a timestamp string into a DateTime object using specific formats.
 * PARAMETERS: string timestampString -> Timestamp string to parse.
 * RETURN: DateTime -> Parsed DateTime value or DateTime.MinValue if parsing fails.
 */
        private static DateTime TryParseTimestamp(string timestampString)
        {
            // Replace underscores with slashes for compatibility
            string cleanedTimestamp = timestampString.Replace('_', '/');

            // Define the custom format
            string format = "M/d/yyyy HH:mm:ss";

            // Try parsing the cleaned timestamp
            if (DateTime.TryParseExact(cleanedTimestamp, format, null, System.Globalization.DateTimeStyles.None, out DateTime parsedTimestamp))
            {
                return parsedTimestamp;
            }

            // Log and return default value on failure
            Console.WriteLine($"Invalid timestamp format: {timestampString}. Defaulting to DateTime.MinValue.");
            return DateTime.MinValue;
        }

    }
}
