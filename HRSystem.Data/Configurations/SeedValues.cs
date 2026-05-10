namespace HRSystem.Data.Configurations;

/// <summary>Fixed values for EF HasData seeding (never DateTime.UtcNow).</summary>
internal static class SeedValues
{
    internal static readonly DateTime EmployeeAndDepartmentCreatedAt = new(2025, 1, 1, 8, 0, 0, DateTimeKind.Utc);
    internal static readonly DateOnly EmployeeHireDate = new(2024, 6, 1);

    internal static readonly DateTime TaskCreatedAt1 = new(2025, 2, 10, 9, 0, 0, DateTimeKind.Utc);
    internal static readonly DateTime TaskCreatedAt2 = new(2025, 2, 11, 9, 30, 0, DateTimeKind.Utc);
    internal static readonly DateTime TaskUpdatedAt1 = new(2025, 2, 15, 14, 0, 0, DateTimeKind.Utc);

    internal static readonly DateTime PayrollCreatedAt1 = new(2025, 3, 1, 10, 0, 0, DateTimeKind.Utc);
    internal static readonly DateTime PayrollProcessedAt1 = new(2025, 3, 2, 11, 0, 0, DateTimeKind.Utc);

    internal static readonly DateTime LeaveRequestDate1 = new(2025, 2, 20, 12, 0, 0, DateTimeKind.Utc);
    internal static readonly DateTime LeaveApprovedAt1 = new(2025, 2, 21, 9, 0, 0, DateTimeKind.Utc);

    internal static readonly DateTime AttendanceCheckIn1 = new(2025, 3, 10, 8, 0, 0, DateTimeKind.Utc);
    internal static readonly DateTime AttendanceCheckOut1 = new(2025, 3, 10, 16, 30, 0, DateTimeKind.Utc);
}
