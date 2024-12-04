/*
* FILE : DatabaseService.cs
* PROJECT : SENG3020 - Milestone #2 
* PROGRAMMER : Shivang Chordia, Keval PAtel, Urvish Motivaras & Jaygiri Goswami
* DATE : 2024-11-22
* DESCRIPTION : This DatabaseService class provides the functionality for interacting with a database, specifically for storing and querying telemetry data in the Ground Terminal Management System. 
*/


using System.Data;
using Backend_Ground_Terminal.Model;
using Microsoft.Data.SqlClient;

namespace Ground_Terminal_Management_System.Services
{
    public class DatabaseService
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private static bool _tablesChecked = false; // Flag to check if tables were already created
        private static readonly object _lock = new object();
        private readonly ILogger<DatabaseService> _logger;

        /*
         * CONSTRUCTOR: Database Service
         * DESCRIPTION: Private constructor to prevent direct instantiation
         * PARAMETERS: IConfiguration configuration
         * RETURN: n/a
         */
        private DatabaseService(IConfiguration configuration, ILogger<DatabaseService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _connectionString = _configuration.GetConnectionString("DefaultConnection")
              ?? throw new InvalidOperationException("DefaultConnection is not configured.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            EnsureTableExists().Wait(); // Ensure tables exist during initialization (asynchronously)
        }

        // Public method to initialize and get the instance of DatabaseService
        public static DatabaseService? Instance { get; private set; }

        /*
         * FUNCTION: Initialize
         * DESCRIPTION: A static method to initialize the singleton instance of DatabaseService. It ensures that the Instance is created only once, even if multiple threads try to initialize it 
                        simultaneously.
         * PARAMETERS: IConfiguration configuration -> This parameter is of type IConfiguration, which is an interface provided by ASP.NET Core to access configuration settings
         * RETURN: void
         */
        public static void Initialize(IConfiguration configuration, ILogger<DatabaseService> logger)
        {
            lock (_lock)
            {
                if (Instance == null)
                {
                    Instance = new DatabaseService(configuration, logger);
                }
            }
        }

        /*
        * FUNCTION: EnsureTableExists()
        * DESCRIPTION: This method ensures that the required datbase table exists, and it creates them if they dont exist.
        * PARAMETERS: none
        * RETURN: void
        */
        public async Task EnsureTableExists()
        {
            if (_tablesChecked) // Check if the tables have already been created
                return;

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

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
                        await command.ExecuteNonQueryAsync();
                    }
                }

                _tablesChecked = true; // Set the flag to true after the tables have been created
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while ensuring table existence.");
            }
        }

        /*
        * FUNCTION: StoreTelemetryData()
        * DESCRIPTION: The StoreTelemetryData method saves telemetry data into two database tables created in the EnsureTableExists method(), GForceParameters and AttitudeParameters.
        * PARAMETERS: TelemetryDataModel data -> Telemetry data object passed into the method that contains all the telemetry parameters (e.g., TailNumber, SequenceNumber, Altitude, Pitch, etc.).
        * RETURN: int -> Returns 1 if data was successfully stored into the database & -1 if not.
        */
        public async Task<int> StoreTelemetryDataAsync(TelemetryDataModel data)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Start a transaction
                    using (var transaction = await connection.BeginTransactionAsync())
                    {
                        // Insert data into GForceParameters
                        string insertGForceQuery = @"
                            INSERT INTO GForceParameters 
                            (TailNumber, SequenceNumber, Timestamp, X, Y, Z, Weight)
                            VALUES (@TailNumber, @SequenceNumber, @Timestamp, @X, @Y, @Z, @Weight)";

                        using (var gForceCommand = new SqlCommand(insertGForceQuery, connection, (SqlTransaction)transaction))
                        {
                            gForceCommand.Parameters.Add("@TailNumber", SqlDbType.NVarChar).Value = data.TailNumber;
                            gForceCommand.Parameters.Add("@SequenceNumber", SqlDbType.Int).Value = data.SequenceNumber;
                            gForceCommand.Parameters.Add("@Timestamp", SqlDbType.DateTime2).Value = DateTime.Now;
                            gForceCommand.Parameters.Add("@X", SqlDbType.Float).Value = data.X;
                            gForceCommand.Parameters.Add("@Y", SqlDbType.Float).Value = data.Y;
                            gForceCommand.Parameters.Add("@Z", SqlDbType.Float).Value = data.Z;
                            gForceCommand.Parameters.Add("@Weight", SqlDbType.Float).Value = data.Weight;

                            await gForceCommand.ExecuteNonQueryAsync();
                        }

                        // Insert data into AttitudeParameters
                        string insertAttitudeQuery = @"
                            INSERT INTO AttitudeParameters 
                            (TailNumber, SequenceNumber, Timestamp, Altitude, Pitch, Bank)
                            VALUES (@TailNumber, @SequenceNumber, @Timestamp, @Altitude, @Pitch, @Bank)";

                        using (var attitudeCommand = new SqlCommand(insertAttitudeQuery, connection, (SqlTransaction)transaction))
                        {
                            attitudeCommand.Parameters.Add("@TailNumber", SqlDbType.NVarChar).Value = data.TailNumber;
                            attitudeCommand.Parameters.Add("@SequenceNumber", SqlDbType.Int).Value = data.SequenceNumber;
                            attitudeCommand.Parameters.Add("@Timestamp", SqlDbType.DateTime2).Value = DateTime.Now;
                            attitudeCommand.Parameters.Add("@Altitude", SqlDbType.Float).Value = data.Altitude;
                            attitudeCommand.Parameters.Add("@Pitch", SqlDbType.Float).Value = data.Pitch;
                            attitudeCommand.Parameters.Add("@Bank", SqlDbType.Float).Value = data.Bank;

                            await attitudeCommand.ExecuteNonQueryAsync();
                        }

                        // Commit the transaction
                        await transaction.CommitAsync();
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing telemetry data.");
                return -1;
            }
        }


        /*
       * FUNCTION: SearchTelemetryData()
       * DESCRIPTION: This method fetches telemetry data for a specific aircraft identified by tailNumber, merging records from the two related tables and ensuring no duplicate entries exist.
       * PARAMETERS: string tailNumber -> Tail Number of the flight
       * RETURN: TelemetryDataModel -> Returns the final list of telemetry data models.
       */
        public async Task<List<TelemetryDataModel>> SearchTelemetryDataAsync(string tailNumber)
        {
            var telemetryData = new List<TelemetryDataModel>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

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

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                telemetryData.Add(new TelemetryDataModel
                                {
                                    TailNumber = reader["TailNumber"].ToString(),
                                    SequenceNumber = Convert.ToInt32(reader["SequenceNumber"]),
                                    Timestamp = reader["Timestamp"].ToString(),
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
                _logger.LogError(ex, "Error fetching telemetry data.");
            }

            return telemetryData;
        }
    }
}
