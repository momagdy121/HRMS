using HRSystem.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRSystem.Data.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> entity)
    {
        entity.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
        entity.Property(x => x.LastName).IsRequired().HasMaxLength(100);
        entity.Property(x => x.Email).IsRequired().HasMaxLength(150);
        entity.HasIndex(x => x.Email).IsUnique();
        entity.Property(x => x.Salary).HasPrecision(18, 2);
        entity.Property(x => x.IsHR).HasDefaultValue(false);
        entity.Property(x => x.IsActive).HasDefaultValue(true);
        entity.Property(x => x.IsDeleted).HasDefaultValue(false);
        entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        entity.HasOne<Department>()
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasData(GetSeedEmployees());
    }

    private static Employee[] GetSeedEmployees() =>
    [
        new()
        {
            Id = 1,
            FirstName = "mohamed",
            LastName = "magdy",
            Email = "admin@hr.com",
            IsHR = true,
            DepartmentId = 2,
            Salary = 95_000m,
            HireDate = SeedValues.EmployeeHireDate,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = SeedValues.EmployeeAndDepartmentCreatedAt
        },
        new()
        {
            Id = 2,
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane@hr.com",
            IsHR = false,
            DepartmentId = 1,
            Salary = 82_000m,
            HireDate = SeedValues.EmployeeHireDate,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = SeedValues.EmployeeAndDepartmentCreatedAt
        },
        new()
        {
            Id = 3,
            FirstName = "Bob",
            LastName = "Jones",
            Email = "bob@hr.com",
            IsHR = false,
            DepartmentId = 2,
            Salary = 83_000m,
            HireDate = SeedValues.EmployeeHireDate,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = SeedValues.EmployeeAndDepartmentCreatedAt
        },
        new()
        {
            Id = 4,
            FirstName = "Alice",
            LastName = "Brown",
            Email = "alice@it.com",
            IsHR = false,
            DepartmentId = 1,
            Salary = 62_000m,
            HireDate = SeedValues.EmployeeHireDate,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = SeedValues.EmployeeAndDepartmentCreatedAt
        },
        new()
        {
            Id = 5,
            FirstName = "Charlie",
            LastName = "Wilson",
            Email = "charlie@hr.com",
            IsHR = false,
            DepartmentId = 2,
            Salary = 61_000m,
            HireDate = SeedValues.EmployeeHireDate,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = SeedValues.EmployeeAndDepartmentCreatedAt
        },
        new()
        {
            Id = 6,
            FirstName = "Diana",
            LastName = "Lee",
            Email = "diana@it.com",
            IsHR = false,
            DepartmentId = 1,
            Salary = 63_000m,
            HireDate = SeedValues.EmployeeHireDate,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = SeedValues.EmployeeAndDepartmentCreatedAt
        },
        new()
        {
            Id = 7,
            FirstName = "Evan",
            LastName = "Clark",
            Email = "evan@hr.com",
            IsHR = false,
            DepartmentId = 2,
            Salary = 64_000m,
            HireDate = SeedValues.EmployeeHireDate,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = SeedValues.EmployeeAndDepartmentCreatedAt
        },
        new()
        {
            Id = 8,
            FirstName = "Fiona",
            LastName = "Hall",
            Email = "fiona@it.com",
            IsHR = false,
            DepartmentId = 1,
            Salary = 65_000m,
            HireDate = SeedValues.EmployeeHireDate,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = SeedValues.EmployeeAndDepartmentCreatedAt
        },
        new()
        {
            Id = 9,
            FirstName = "George",
            LastName = "Young",
            Email = "george@hr.com",
            IsHR = false,
            DepartmentId = 2,
            Salary = 66_000m,
            HireDate = SeedValues.EmployeeHireDate,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = SeedValues.EmployeeAndDepartmentCreatedAt
        },
        new()
        {
            Id = 10,
            FirstName = "Hannah",
            LastName = "King",
            Email = "hannah@it.com",
            IsHR = false,
            DepartmentId = 1,
            Salary = 67_000m,
            HireDate = SeedValues.EmployeeHireDate,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = SeedValues.EmployeeAndDepartmentCreatedAt
        },
        new()
        {
            Id = 11,
            FirstName = "Ian",
            LastName = "Wright",
            Email = "ian@hr.com",
            IsHR = false,
            DepartmentId = 2,
            Salary = 68_000m,
            HireDate = SeedValues.EmployeeHireDate,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = SeedValues.EmployeeAndDepartmentCreatedAt
        },
        new()
        {
            Id = 12,
            FirstName = "Julia",
            LastName = "Scott",
            Email = "julia@it.com",
            IsHR = false,
            DepartmentId = 1,
            Salary = 69_000m,
            HireDate = SeedValues.EmployeeHireDate,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = SeedValues.EmployeeAndDepartmentCreatedAt
        },
        new()
        {
            Id = 13,
            FirstName = "Kevin",
            LastName = "Green",
            Email = "kevin@hr.com",
            IsHR = false,
            DepartmentId = 2,
            Salary = 70_000m,
            HireDate = SeedValues.EmployeeHireDate,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = SeedValues.EmployeeAndDepartmentCreatedAt
        },
        new()
        {
            Id = 14,
            FirstName = "Laura",
            LastName = "Adams",
            Email = "laura@it.com",
            IsHR = false,
            DepartmentId = 1,
            Salary = 71_000m,
            HireDate = SeedValues.EmployeeHireDate,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = SeedValues.EmployeeAndDepartmentCreatedAt
        },
        new()
        {
            Id = 15,
            FirstName = "Michael",
            LastName = "Baker",
            Email = "michael@hr.com",
            IsHR = false,
            DepartmentId = 2,
            Salary = 72_000m,
            HireDate = SeedValues.EmployeeHireDate,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = SeedValues.EmployeeAndDepartmentCreatedAt
        }
    ];
}
