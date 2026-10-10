using HRSystem.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRSystem.Data.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> entity)
    {
        entity.ToTable("Employees");

        entity.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
        entity.Property(x => x.LastName).IsRequired().HasMaxLength(100);
        entity.Property(x => x.Email).IsRequired().HasMaxLength(150);
        entity.HasIndex(x => x.Email).IsUnique();
        entity.Property(x => x.Salary).HasPrecision(18, 2);
        entity.Property(x => x.IsHR).HasDefaultValue(false);
        entity.Property(x => x.IsActive).HasDefaultValue(true);
        entity.Property(x => x.IsDeleted).HasDefaultValue(false);
        entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        entity.Property(x => x.IsPasswordChangeRequired).HasDefaultValue(false);

        entity.HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasData(GetSeedEmployees());
    }

    private static Employee[] GetSeedEmployees()
    {
        (int Id, string First, string Last, string Email, bool IsHr, int DeptId, decimal Salary)[] defs =
        [
            (1, "mohamed", "magdy", "admin@hr.com", true, 2, 95_000m),
            (2, "Jane", "Smith", "jane@hr.com", false, 1, 82_000m),
            (3, "Bob", "Jones", "bob@hr.com", false, 2, 83_000m),
            (4, "Alice", "Brown", "alice@it.com", false, 1, 62_000m),
            (5, "Charlie", "Wilson", "charlie@hr.com", false, 2, 61_000m),
            (6, "Diana", "Lee", "diana@it.com", false, 1, 63_000m),
            (7, "Evan", "Clark", "evan@hr.com", false, 2, 64_000m),
            (8, "Fiona", "Hall", "fiona@it.com", false, 1, 65_000m),
            (9, "George", "Young", "george@hr.com", false, 2, 66_000m),
            (10, "Hannah", "King", "hannah@it.com", false, 1, 67_000m),
            (11, "Ian", "Wright", "ian@hr.com", false, 2, 68_000m),
            (12, "Julia", "Scott", "julia@it.com", false, 1, 69_000m),
            (13, "Kevin", "Green", "kevin@hr.com", false, 2, 70_000m),
            (14, "Laura", "Adams", "laura@it.com", false, 1, 71_000m),
            (15, "Michael", "Baker", "michael@hr.com", false, 2, 72_000m)
        ];

        return defs.Select(d => new Employee
        {
            Id = d.Id,
            FirstName = d.First,
            LastName = d.Last,
            Email = d.Email,
            NormalizedEmail = d.Email.ToUpperInvariant(),
            UserName = d.Email,
            NormalizedUserName = d.Email.ToUpperInvariant(),
            EmailConfirmed = true,
            SecurityStamp = $"00000000-0000-0000-0000-{d.Id:D12}",
            ConcurrencyStamp = $"00000000-0000-0000-0000-{d.Id:D12}",
            IsHR = d.IsHr,
            DepartmentId = d.DeptId,
            Salary = d.Salary,
            HireDate = SeedValues.EmployeeHireDate,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = SeedValues.EmployeeAndDepartmentCreatedAt,
            IsPasswordChangeRequired = false
        }).ToArray();
    }
}
