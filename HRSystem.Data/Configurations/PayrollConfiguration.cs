using HRSystem.Common.Enums;
using HRSystem.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRSystem.Data.Configurations;

public class PayrollConfiguration : IEntityTypeConfiguration<Payroll>
{
    public void Configure(EntityTypeBuilder<Payroll> entity)
    {
        entity.Property(x => x.BaseSalary).HasPrecision(18, 2);
        entity.Property(x => x.TotalBonus).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.TotalDeduction).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.NetSalary).HasPrecision(18, 2);
        entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        entity.HasIndex(x => new { x.EmployeeId, x.Month, x.Year }).IsUnique();

        entity.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(x => x.ProcessedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // NetSalary = BaseSalary + TotalBonus - TotalDeduction (snapshot at processing time).
        entity.HasData(
            new Payroll
            {
                Id = 1,
                EmployeeId = 4,
                BaseSalary = 62_000m,
                TotalBonus = 1_000m,
                TotalDeduction = 200m,
                NetSalary = 62_800m,
                Month = 4,
                Year = 2025,
                Status = PayrollStatus.Draft,
                ProcessedBy = 1,
                ProcessedAt = null,
                CreatedAt = SeedValues.PayrollCreatedAt1
            },
            new Payroll
            {
                Id = 2,
                EmployeeId = 5,
                BaseSalary = 61_000m,
                TotalBonus = 500m,
                TotalDeduction = 150m,
                NetSalary = 61_350m,
                Month = 5,
                Year = 2025,
                Status = PayrollStatus.Approved,
                ProcessedBy = 1,
                ProcessedAt = SeedValues.PayrollProcessedAt1,
                CreatedAt = SeedValues.PayrollCreatedAt1
            });
    }
}
