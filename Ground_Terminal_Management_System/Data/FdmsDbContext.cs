using Microsoft.EntityFrameworkCore;

namespace Ground_Terminal_Management_System.Data
{
    public class FdmsDbContext : DbContext
    {
        public FdmsDbContext(DbContextOptions<FdmsDbContext> options) : base(options) { }

        // DbSet property for TelemetryData
        public DbSet<TelemetryData> TelemetryRecords { get; set; }

        // Class for TelemetryData
        public class TelemetryData
        {
            public int Id { get; set; } // Primary Key
            public string AircraftTailNumber { get; set; }
            public DateTime Timestamp { get; set; }

            // GForce specific fields
            public double? AccelX { get; set; }
            public double? AccelY { get; set; }
            public double? AccelZ { get; set; }

            // Attitude specific fields
            public double? Altitude { get; set; }
            public double? Pitch { get; set; }
            public double? Bank { get; set; }
        }
    }
}
