using HRSystem.Business.DTOs.Attendance;
using HRSystem.Data.Models;

namespace HRSystem.Business.Mapping;

public static class AttendanceMapper
{
    public static Attendance FromDto(MarkTeamAttendanceDto dto) =>
        new()
        {
            EmployeeId = dto.EmployeeId,
            Date = dto.Date,
            CheckInTime = dto.CheckInTime,
            CheckOutTime = dto.CheckOutTime,
            Notes = dto.Notes?.Trim(),
            IsDeleted = false
        };

    public static void UpdateFromDto(Attendance attendance, MarkTeamAttendanceDto dto)
    {
        attendance.CheckInTime = dto.CheckInTime;
        attendance.CheckOutTime = dto.CheckOutTime;
        attendance.Notes = dto.Notes?.Trim();
    }
}
