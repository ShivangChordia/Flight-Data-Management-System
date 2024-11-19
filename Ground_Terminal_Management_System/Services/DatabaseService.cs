using Ground_Terminal_Management_System.Model;
using Microsoft.Data.SqlClient;


namespace Ground_Terminal_Management_System.Services
{
    public static class DatabaseService
    {
        private static readonly string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["TelemetryDb"].ConnectionString;


        public static void EnsureTableExists()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string createTableQuery = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TelemetryData' AND xtype='U')
                CREATE TABLE TelemetryData (
                    Id INT IDENTITY PRIMARY KEY,
                    TailNumber NVARCHAR(50),
                    SequenceNumber INT,
                    Timestamp NVARCHAR(50),
                    X FLOAT,
                    Y FLOAT,
                    Z FLOAT,
                    Altitude FLOAT,
                    Pitch FLOAT,
                    Bank FLOAT,
                    Checksum INT
                )";
                var command = new SqlCommand(createTableQuery, connection);
                command.ExecuteNonQuery();
            }
        }

        public static void StoreTelemetryData(TelemetryDataModel data)
        {
            EnsureTableExists();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string insertQuery = @"
                INSERT INTO TelemetryData 
                (TailNumber, SequenceNumber, Timestamp, X, Y, Z, Altitude, Pitch, Bank, Checksum)
                VALUES (@TailNumber, @SequenceNumber, @Timestamp, @X, @Y, @Z, @Altitude, @Pitch, @Bank, @Checksum)";
                var command = new SqlCommand(insertQuery, connection);

                command.Parameters.AddWithValue("@TailNumber", data.TailNumber);
                command.Parameters.AddWithValue("@SequenceNumber", data.SequenceNumber);
                command.Parameters.AddWithValue("@Timestamp", data.Timestamp);
                command.Parameters.AddWithValue("@X", data.X);
                command.Parameters.AddWithValue("@Y", data.Y);
                command.Parameters.AddWithValue("@Z", data.Z);
                command.Parameters.AddWithValue("@Altitude", data.Altitude);
                command.Parameters.AddWithValue("@Pitch", data.Pitch);
                command.Parameters.AddWithValue("@Bank", data.Bank);
                command.Parameters.AddWithValue("@Checksum", data.Checksum);

                command.ExecuteNonQuery();
            }
        }
    }
}