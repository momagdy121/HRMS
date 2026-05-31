using HRSystem.Data.Models;

namespace HRSystem.Business.Helpers;

public static class AttendanceWorkflow
{
    public static Attendance CreateForCheckIn(int employeeId, DateOnly date, DateTime checkInTime) =>
        new()
        {
            EmployeeId = employeeId,
            Date = date,
            CheckInTime = checkInTime,
            IsDeleted = false
        };

    public static void ApplyCheckIn(Attendance attendance, DateTime checkInTime) =>
        attendance.CheckInTime = checkInTime;

    public static void ApplyCheckOut(Attendance attendance, DateTime checkOutTime) =>
        attendance.CheckOutTime = checkOutTime;
}
