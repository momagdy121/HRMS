using HRSystem.Common.Enums;
using HRSystem.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRSystem.Data.Configurations;

public class LeaveBalanceConfiguration : IEntityTypeConfiguration<LeaveBalance>
{
    public void Configure(EntityTypeBuilder<LeaveBalance> entity)
    {
        entity.Property(x => x.UsedDays).HasDefaultValue(0);
        entity.HasIndex(x => new { x.EmployeeId, x.Year, x.LeaveType }).IsUnique();

        entity.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasData(BuildLeaveBalances());
    }

    private static LeaveBalance[] BuildLeaveBalances()
    {
        const int year = 2025;
        var rows = new List<LeaveBalance>();
        var id = 1;
        for (var employeeId = 1; employeeId <= 15; employeeId++)
        {
            rows.Add(new LeaveBalance
            {
                Id = id++,
                EmployeeId = employeeId,
                Year = year,
                LeaveType = LeaveType.Annual,
                TotalDays = 20,
                UsedDays = 0
            });
            rows.Add(new LeaveBalance
            {
                Id = id++,
                EmployeeId = employeeId,
                Year = year,
                LeaveType = LeaveType.Sick,
                TotalDays = 10,
                UsedDays = 0
            });
        }

        return rows.ToArray();
    }
}
