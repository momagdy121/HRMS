using HRSystem.Business.DTOs;
using HRSystem.Business.DTOs.Attendance;
using HRSystem.Data.Models;

namespace HRSystem.Business.Interfaces.Services;

public interface IAttendanceService
{
    Task<Attendance> CheckInAsync(CancellationToken cancellationToken = default);

    Task<Attendance> CheckOutAsync(CancellationToken cancellationToken = default);

    Task<Attendance> MarkTeamAttendanceAsync(MarkTeamAttendanceDto dto, CancellationToken cancellationToken = default);

    Task<PagedResult<Attendance>> GetByEmployeeAsync(int employeeId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);

    Task<PagedResult<Attendance>> GetByDepartmentAsync(int departmentId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);

    Task<PagedResult<Attendance>> GetReportAsync(
        DateOnly date,
        int? departmentId,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);
}
