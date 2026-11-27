using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MVCS.Models.Organization;

namespace BaseEFAPI.MVCS.Services.Context;

public sealed class NexusDbContext(DbContextOptions<NexusDbContext> options)
    : DbContext(options)
{
    public DbSet<AgencyAdministrator> AgencyAdministrator => Set<AgencyAdministrator>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        EntityTypeBuilder<AgencyAdministrator> entity = modelBuilder.Entity<AgencyAdministrator>();
        entity.ToTable("AgencyAdministrator");
        entity.Property(x => x.RecordedAtUtc).HasColumnName("TimeStampUTC");

        entity.ComplexProperty(x => x.Agency, details =>
        {
            details.IsRequired();
            details.Property(x => x.CDSCode).HasColumnName("CDSCode");
            details.Property(x => x.OrganizationType).HasColumnName("OrganizationType");
            details.Property(x => x.Name).HasColumnName("AgencyName");
            details.Property(x => x.Address).HasColumnName("AgencyAddress");
            details.Property(x => x.ActiveStatus).HasColumnName("ActiveStatus");
            details.Property(x => x.IsActive).HasColumnName("IsActive");
            details.Property(x => x.Website).HasColumnName("Website");
            details.Property(x => x.Email).HasColumnName("AgencyEmail");
        });
        entity.ComplexProperty(x => x.Person, details =>
        {
            details.IsRequired();
            details.Property(x => x.FirstName).HasColumnName("FirstName");
            details.Property(x => x.MiddleName).HasColumnName("MiddleName");
            details.Property(x => x.LastName).HasColumnName("LastName");
            details.Property(x => x.Suffix).HasColumnName("Suffix");
            details.Property(x => x.Title).HasColumnName("Title");
            details.Property(x => x.PhoneNumber).HasColumnName("PhoneNumber");
            details.Property(x => x.PhoneExtension).HasColumnName("PhoneExtension");
            details.Property(x => x.Email).HasColumnName("PersonEmail");
        });
    }
}
