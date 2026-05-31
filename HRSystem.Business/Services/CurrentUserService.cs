using System.Security.Claims;
using HRSystem.Business.Exceptions;
using HRSystem.Business.Interfaces.Services;
using HRSystem.Common.Constants;
using HRSystem.Data.Interfaces;
using HRSystem.Data.Models;
using Microsoft.AspNetCore.Http;

namespace HRSystem.Business.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IApplicationUserRepository _applicationUsers;
    private readonly IEmployeeRepository _employees;
    private Employee? _cachedEmployee;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        IApplicationUserRepository applicationUsers,
        IEmployeeRepository employees)
    {
        _httpContextAccessor = httpContextAccessor;
        _applicationUsers = applicationUsers;
        _employees = employees;
    }

    public int GetCurrentUserId()
    {
        var idValue = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(idValue) || !int.TryParse(idValue, out var userId))
            throw new UnauthorizedException();

        return userId;
    }

    public bool IsHR() =>
        _httpContextAccessor.HttpContext?.User.IsInRole(RoleNames.HR) ?? false;

    public bool IsDepartmentHead() =>
        _httpContextAccessor.HttpContext?.User.IsInRole(RoleNames.DepartmentHead) ?? false;

    public async Task<Employee> GetCurrentEmployeeAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedEmployee != null)
            return _cachedEmployee;

        var userId = GetCurrentUserId();
        var user = await _applicationUsers.GetByIdAsync(userId, cancellationToken)
                   ?? throw new UnauthorizedException();

        var employee = await _employees.GetByIdAsync(user.EmployeeId, cancellationToken)
                       ?? throw new NotFoundException("Employee record not found.");

        _cachedEmployee = employee;
        return employee;
    }
}
