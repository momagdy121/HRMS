using HRSystem.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRSystem.Data.Configurations;

public class UserNotificationConfiguration : IEntityTypeConfiguration<UserNotification>
{
    public void Configure(EntityTypeBuilder<UserNotification> entity)
    {
        entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Message).IsRequired().HasMaxLength(500);
        entity.Property(x => x.LinkUrl).HasMaxLength(500);
        entity.Property(x => x.IsRead).HasDefaultValue(false);
        entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        entity.HasIndex(x => new { x.EmployeeId, x.IsRead, x.CreatedAt });

        entity.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
