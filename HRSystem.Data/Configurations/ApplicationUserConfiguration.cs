using HRSystem.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRSystem.Data.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> entity)
    {
        entity.Property(x => x.Email).IsRequired().HasMaxLength(256);
        entity.Property(x => x.IsPasswordChangeRequired).HasDefaultValue(true);

        entity.HasIndex(x => x.EmployeeId).IsUnique();

        entity.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
