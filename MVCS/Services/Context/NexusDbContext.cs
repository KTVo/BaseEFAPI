using Microsoft.EntityFrameworkCore;
using MVCS.Models.Organization;

namespace BaseEFAPI.MVCS.Services.Context;

public sealed class NexusDbContext(DbContextOptions<NexusDbContext> options)
    : DbContext(options)
{
    public DbSet<AgencyAdministrator> AgencyAdministrator => Set<AgencyAdministrator>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
