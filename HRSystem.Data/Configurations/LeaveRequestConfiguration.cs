using HRSystem.Common.Enums;
using HRSystem.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRSystem.Data.Configurations;

public class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> entity)
    {
        entity.Property(x => x.Reason).HasMaxLength(500);
        entity.Property(x => x.RejectionReason).HasMaxLength(500);
        entity.Property(x => x.RequestDate).HasDefaultValueSql("GETUTCDATE()");

        entity.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(x => x.ApprovedBy)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasData(
            new LeaveRequest
            {
                Id = 1,
                EmployeeId = 4,
                StartDate = new DateOnly(2025, 8, 4),
                EndDate = new DateOnly(2025, 8, 8),
                LeaveType = LeaveType.Annual,
                Reason = "Summer vacation",
                Status = LeaveRequestStatus.Pending,
                RequestDate = SeedValues.LeaveRequestDate1,
                ApprovedBy = null,
                ApprovedAt = null,
                RejectionReason = null
            },
            new LeaveRequest
            {
                Id = 2,
                EmployeeId = 5,
                StartDate = new DateOnly(2025, 7, 1),
                EndDate = new DateOnly(2025, 7, 3),
                LeaveType = LeaveType.Sick,
                Reason = null,
                Status = LeaveRequestStatus.Approved,
                RequestDate = SeedValues.LeaveRequestDate1,
                ApprovedBy = 1,
                ApprovedAt = SeedValues.LeaveApprovedAt1,
                RejectionReason = null
            },
            new LeaveRequest
            {
                Id = 3,
                EmployeeId = 6,
                StartDate = new DateOnly(2025, 9, 10),
                EndDate = new DateOnly(2025, 9, 12),
                LeaveType = LeaveType.Unpaid,
                Reason = "Personal errands",
                Status = LeaveRequestStatus.Rejected,
                RequestDate = SeedValues.LeaveRequestDate1,
                ApprovedBy = null,
                ApprovedAt = null,
                RejectionReason = "Team coverage not available for those dates."
            },
            new LeaveRequest
            {
                Id = 4,
                EmployeeId = 8,
                StartDate = new DateOnly(2025, 10, 1),
                EndDate = new DateOnly(2025, 10, 5),
                LeaveType = LeaveType.Annual,
                Reason = "Conference travel",
                Status = LeaveRequestStatus.Pending,
                RequestDate = SeedValues.LeaveRequestDate1,
                ApprovedBy = null,
                ApprovedAt = null,
                RejectionReason = null
            });
    }
}
