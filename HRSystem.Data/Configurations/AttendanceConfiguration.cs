using HRSystem.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRSystem.Data.Configurations;

public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
{
    public void Configure(EntityTypeBuilder<Attendance> entity)
    {
        entity.Property(x => x.Notes).HasMaxLength(200);
        entity.Property(x => x.IsDeleted).HasDefaultValue(false);
        entity.HasIndex(x => new { x.EmployeeId, x.Date }).IsUnique();

        entity.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasData(
            new Attendance
            {
                Id = 1,
                EmployeeId = 4,
                Date = new DateOnly(2025, 3, 10),
                CheckInTime = SeedValues.AttendanceCheckIn1,
                CheckOutTime = SeedValues.AttendanceCheckOut1,
                Notes = null,
                IsDeleted = false
            },
            new Attendance
            {
                Id = 2,
                EmployeeId = 5,
                Date = new DateOnly(2025, 3, 11),
                CheckInTime = new DateTime(2025, 3, 11, 8, 15, 0, DateTimeKind.Utc),
                CheckOutTime = new DateTime(2025, 3, 11, 16, 45, 0, DateTimeKind.Utc),
                Notes = null,
                IsDeleted = false
            },
            new Attendance
            {
                Id = 3,
                EmployeeId = 6,
                Date = new DateOnly(2025, 3, 12),
                CheckInTime = new DateTime(2025, 3, 12, 7, 55, 0, DateTimeKind.Utc),
                CheckOutTime = null,
                Notes = "Forgot checkout — corrected next day.",
                IsDeleted = false
            },
            new Attendance
            {
                Id = 4,
                EmployeeId = 2,
                Date = new DateOnly(2025, 3, 13),
                CheckInTime = new DateTime(2025, 3, 13, 8, 0, 0, DateTimeKind.Utc),
                CheckOutTime = new DateTime(2025, 3, 13, 17, 0, 0, DateTimeKind.Utc),
                Notes = null,
                IsDeleted = false
            },
            new Attendance
            {
                Id = 5,
                EmployeeId = 3,
                Date = new DateOnly(2025, 3, 14),
                CheckInTime = new DateTime(2025, 3, 14, 8, 30, 0, DateTimeKind.Utc),
                CheckOutTime = new DateTime(2025, 3, 14, 16, 0, 0, DateTimeKind.Utc),
                Notes = null,
                IsDeleted = false
            },
            new Attendance
            {
                Id = 6,
                EmployeeId = 1,
                Date = new DateOnly(2025, 3, 17),
                CheckInTime = new DateTime(2025, 3, 17, 7, 45, 0, DateTimeKind.Utc),
                CheckOutTime = new DateTime(2025, 3, 17, 15, 30, 0, DateTimeKind.Utc),
                Notes = "Half day",
                IsDeleted = false
            });
    }
}
