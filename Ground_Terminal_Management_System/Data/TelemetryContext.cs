using Microsoft.EntityFrameworkCore;

namespace Ground_Terminal_Management_System.Data
{
    public class TelemetryContext : DbContext
    {
        // DbSet properties for the tables in the database
        public DbSet<GForceParameter> GForceParameters { get; set; }
        public DbSet<AttitudeParameter> AttitudeParameters { get; set; }

        // Constructor to configure options for the DbContext
        public TelemetryContext(DbContextOptions<TelemetryContext> options)
            : base(options) { }

        // Class for GForceParameter table
        public class GForceParameter
        {
            public string AircraftTailNumber { get; set; }
            public string Timestamp { get; set; }
            public double AccelX { get; set; }
            public double AccelY { get; set; }
            public double AccelZ { get; set; }
        }

        // Class for AttitudeParameter table
        public class AttitudeParameter
        {
            public double Altitude { get; set; }
            public double Pitch { get; set; }
            public double Bank { get; set; }
        }
    }
}
