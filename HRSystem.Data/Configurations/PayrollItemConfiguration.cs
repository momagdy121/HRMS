using HRSystem.Common.Enums;
using HRSystem.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRSystem.Data.Configurations;

public class PayrollItemConfiguration : IEntityTypeConfiguration<PayrollItem>
{
    public void Configure(EntityTypeBuilder<PayrollItem> entity)
    {
        entity.Property(x => x.Description).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Amount).HasPrecision(18, 2);

        entity.HasOne<Payroll>()
            .WithMany()
            .HasForeignKey(x => x.PayrollId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasData(
            new PayrollItem
            {
                Id = 1,
                PayrollId = 1,
                ItemType = ItemType.Bonus,
                Description = "Q1 performance bonus",
                Amount = 1_000m
            },
            new PayrollItem
            {
                Id = 2,
                PayrollId = 1,
                ItemType = ItemType.Deduction,
                Description = "Health insurance contribution",
                Amount = 200m
            },
            new PayrollItem
            {
                Id = 3,
                PayrollId = 2,
                ItemType = ItemType.Bonus,
                Description = "Team milestone bonus",
                Amount = 500m
            },
            new PayrollItem
            {
                Id = 4,
                PayrollId = 2,
                ItemType = ItemType.Deduction,
                Description = "Parking pass",
                Amount = 150m
            });
    }
}
