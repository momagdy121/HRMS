using HRSystem.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRSystem.Data.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> entity)
    {
        entity.Property(x => x.Name).IsRequired().HasMaxLength(100);
        entity.HasIndex(x => x.Name).IsUnique();
        entity.Property(x => x.IsDeleted).HasDefaultValue(false);
        entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        entity.Property(x => x.ManagerId).IsRequired();

        entity.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(x => x.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasData(
            new Department
            {
                Id = 1,
                Name = "Information Technology",
                ManagerId = 2,
                IsDeleted = false,
                CreatedAt = SeedValues.EmployeeAndDepartmentCreatedAt
            },
            new Department
            {
                Id = 2,
                Name = "Human Resources",
                ManagerId = 3,
                IsDeleted = false,
                CreatedAt = SeedValues.EmployeeAndDepartmentCreatedAt
            });
    }
}
