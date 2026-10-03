using Microsoft.EntityFrameworkCore;

namespace BaseEFAPI.MVCS.Services.Context;

public sealed class RegistrationDbContext(DbContextOptions<RegistrationDbContext> options) : DbContext(options)
{
    // DEFINE DB SETS FOR ENTITIES
    public DbSet<ApplicationUserModel> ApplicationUser { get; set; }




    // ADDITIONAL DB SETS FOR OTHER ENTITIES CAN BE ADDED HERE
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationUserModel>().ToTable("ApplicationUser");
    }
}
