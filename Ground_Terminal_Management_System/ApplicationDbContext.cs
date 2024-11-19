using Ground_Terminal_Management_System.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client.TelemetryCore.TelemetryClient;

namespace Ground_Terminal_Management_System
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<TelemetryDataModel> TelemetryDatas { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }
    }

}
