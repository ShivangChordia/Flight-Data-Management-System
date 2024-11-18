using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client.TelemetryCore.TelemetryClient;


namespace Ground_Terminal_Management_System.Data
{
    public class FdmsDbContext: DbContext
    {
        public FdmsDbContext(DbContextOptions<FdmsDbContext> options) : base(options) { }

        public DbSet<TelemetryData> TelemetryRecords { get; set; }
    }
}
