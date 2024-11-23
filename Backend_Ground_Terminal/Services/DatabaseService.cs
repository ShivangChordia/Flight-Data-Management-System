﻿using Backend_Ground_Terminal.Model;
using System.Data;
using System.Data.SqlClient;

namespace Ground_Terminal_Management_System.Services
{
    public class DatabaseService
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private static bool _tablesChecked = false; // Flag to check if tables were already created
        private static readonly object _lock = new object();

        // Private constructor to prevent direct instantiation
        private DatabaseService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
            EnsureTableExists(); // Ensure tables exist during initialization
        }

        // Public method to initialize and get the instance of DatabaseService
        public static DatabaseService Instance { get; private set; }

        public static void Initialize(IConfiguration configuration)
        {
            lock (_lock)
            {
                if (Instance == null)
                {
                    Instance = new DatabaseService(configuration);
                }
            }
        }

        public void EnsureTableExists()
        {
            if (_tablesChecked) // Check if the tables have already been created
                return;

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string createTableQuery = @"
                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'GForceParameters')
                        BEGIN
                            CREATE TABLE GForceParameters (
                                Id INT IDENTITY PRIMARY KEY,                
                                TailNumber NVARCHAR(50),       
                                SequenceNumber INT,
                                Timestamp DATETIME2,
                                X FLOAT,
                                Y FLOAT,
                                Z FLOAT,
                                Weight FLOAT
                            );
                        END

                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AttitudeParameters')
                        BEGIN
                            CREATE TABLE AttitudeParameters (
                                Id INT IDENTITY PRIMARY KEY, 
                                TailNumber NVARCHAR(50),
                                SequenceNumber INT,
                                Timestamp DATETIME2,
                                Altitude FLOAT,
                                Pitch FLOAT, 
                                Bank FLOAT
                            );
                        END";

                    using (var command = new SqlCommand(createTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                _tablesChecked = true; // Set the flag to true after the tables have been created
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while ensuring table existence: {ex.Message}");
                // Optionally, log this exception or rethrow based on your business logic
            }
        }

        public int StoreTelemetryData(TelemetryDataModel data)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // Insert data into GForceParameters
                    string insertGForceQuery = @"
                INSERT INTO GForceParameters 
                (TailNumber, SequenceNumber, Timestamp, X, Y, Z, Weight)
                VALUES (@TailNumber, @SequenceNumber, @Timestamp, @X, @Y, @Z, @Weight)";

                    using (var gForceCommand = new SqlCommand(insertGForceQuery, connection))
                    {
                        gForceCommand.Parameters.Add("@TailNumber", SqlDbType.NVarChar).Value = data.TailNumber;
                        gForceCommand.Parameters.Add("@SequenceNumber", SqlDbType.Int).Value = data.SequenceNumber;
                        gForceCommand.Parameters.Add("@Timestamp", SqlDbType.DateTime2).Value = DateTime.Now;
                        gForceCommand.Parameters.Add("@X", SqlDbType.Float).Value = data.X;
                        gForceCommand.Parameters.Add("@Y", SqlDbType.Float).Value = data.Y;
                        gForceCommand.Parameters.Add("@Z", SqlDbType.Float).Value = data.Z;
                        gForceCommand.Parameters.Add("@Weight", SqlDbType.Float).Value = data.Weight;

                        gForceCommand.ExecuteNonQuery();
                    }

                    // Insert data into AttitudeParameters
                    string insertAttitudeQuery = @"
                INSERT INTO AttitudeParameters 
                (TailNumber, SequenceNumber, Timestamp, Altitude, Pitch, Bank)
                VALUES (@TailNumber, @SequenceNumber, @Timestamp, @Altitude, @Pitch, @Bank)";

                    using (var attitudeCommand = new SqlCommand(insertAttitudeQuery, connection))
                    {
                        attitudeCommand.Parameters.Add("@TailNumber", SqlDbType.NVarChar).Value = data.TailNumber;
                        attitudeCommand.Parameters.Add("@SequenceNumber", SqlDbType.Int).Value = data.SequenceNumber;
                        attitudeCommand.Parameters.Add("@Timestamp", SqlDbType.DateTime2).Value = DateTime.Now;
                        attitudeCommand.Parameters.Add("@Altitude", SqlDbType.Float).Value = data.Altitude;
                        attitudeCommand.Parameters.Add("@Pitch", SqlDbType.Float).Value = data.Pitch;
                        attitudeCommand.Parameters.Add("@Bank", SqlDbType.Float).Value = data.Bank;

                        attitudeCommand.ExecuteNonQuery();
                    }
                }
                return 1;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error storing telemetry data: {ex.Message}");
                return -1;  
            }
        }

        public List<TelemetryDataModel> SearchTelemetryData(string tailNumber)
        {
            var telemetryData = new List<TelemetryDataModel>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // SQL query to join GForceParameters and AttitudeParameters
                    string query = @"
                SELECT 
                    g.TailNumber, g.SequenceNumber, g.Timestamp, g.X, g.Y, g.Z, g.Weight,
                    a.Altitude, a.Pitch, a.Bank
                FROM GForceParameters g
                INNER JOIN AttitudeParameters a
                ON g.TailNumber = a.TailNumber AND g.SequenceNumber = a.SequenceNumber
                WHERE g.TailNumber = @TailNumber
                ORDER BY g.Timestamp";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@TailNumber", SqlDbType.NVarChar).Value = tailNumber;

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                telemetryData.Add(new TelemetryDataModel
                                {
                                    TailNumber = reader["TailNumber"].ToString(),
                                    SequenceNumber = Convert.ToInt32(reader["SequenceNumber"]),
                                    Timestamp = Convert.ToDateTime(reader["Timestamp"]),
                                    X = (float)Convert.ToDouble(reader["X"]),
                                    Y = (float)Convert.ToDouble(reader["Y"]),
                                    Z = (float)Convert.ToDouble(reader["Z"]),
                                    Weight = (float)Convert.ToDouble(reader["Weight"]),
                                    Altitude = (float)Convert.ToDouble(reader["Altitude"]),
                                    Pitch = (float)Convert.ToDouble(reader["Pitch"]),
                                    Bank = (float)Convert.ToDouble(reader["Bank"])
                                });
                            }
                        }
                    }
                }

                // Remove duplicates based on SequenceNumber and Timestamp
                telemetryData = telemetryData
                    .GroupBy(td => new { td.SequenceNumber, td.Timestamp })
                    .Select(group => group.First())
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching telemetry data: {ex.Message}");
            }

            return telemetryData;
        }




    }
}