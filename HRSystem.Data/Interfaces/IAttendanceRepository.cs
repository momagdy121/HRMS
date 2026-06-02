using HRSystem.Data.Common;
using HRSystem.Data.Models;

namespace HRSystem.Data.Interfaces;

public interface IAttendanceRepository
{
    Task<Attendance?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Attendance?> GetByEmployeeAndDateAsync(int employeeId, DateOnly date, CancellationToken cancellationToken = default);

    Task<PagedList<Attendance>> GetByEmployeePagedAsync(int employeeId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<PagedList<Attendance>> GetByDepartmentPagedAsync(int departmentId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<PagedList<Attendance>> GetReportPagedAsync(
        DateOnly date,
        int? departmentId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<int> SoftDeleteAllForEmployeeAsync(int employeeId, CancellationToken cancellationToken = default);

    Task AddAsync(Attendance attendance, CancellationToken cancellationToken = default);

    void Update(Attendance attendance);
}
