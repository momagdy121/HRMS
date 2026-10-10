using HRSystem.Data.Common;
using HRSystem.Data.Context;
using HRSystem.Data.Interfaces;
using HRSystem.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.Data.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _context;

    public EmployeeRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public Task<Employee?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        _context.Employees.FirstOrDefaultAsync(
            e => e.Email != null && e.Email.ToLower() == email.ToLower(),
            cancellationToken);

    public Task<PagedList<Employee>> GetActivePagedAsync(int page, int pageSize, string? search = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Employees
            .AsNoTracking()
            .Where(e => !e.IsDeleted);

        query = ApplySearch(query, search);

        return query
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToPagedListAsync(page, pageSize, cancellationToken);
    }

    public Task<PagedList<Employee>> GetAllPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default) =>
        _context.Employees
            .AsNoTracking()
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToPagedListAsync(page, pageSize, cancellationToken);

    public Task<PagedList<Employee>> GetByDepartmentPagedAsync(int departmentId, int page, int pageSize, string? search = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Employees
            .AsNoTracking()
            .Where(e => !e.IsDeleted && e.DepartmentId == departmentId);

        query = ApplySearch(query, search);

        return query
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToPagedListAsync(page, pageSize, cancellationToken);
    }

    public Task<PagedList<Employee>> GetDeletedPagedAsync(int page, int pageSize, string? search = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Employees
            .AsNoTracking()
            .Where(e => e.IsDeleted);

        query = ApplySearch(query, search);

        return query
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToPagedListAsync(page, pageSize, cancellationToken);
    }

    public Task<bool> IsManagerOfAnyDepartmentAsync(int employeeId, CancellationToken cancellationToken = default) =>
        _context.Departments.AnyAsync(d => d.ManagerId == employeeId && !d.IsDeleted, cancellationToken);

    public Task<bool> EmailExistsAsync(string email, int? excludeEmployeeId = null, CancellationToken cancellationToken = default)
    {
        var normalized = email.ToLower();
        var query = _context.Employees.Where(e => e.Email != null && e.Email.ToLower() == normalized);
        if (excludeEmployeeId.HasValue)
            query = query.Where(e => e.Id != excludeEmployeeId.Value);

        return query.AnyAsync(cancellationToken);
    }

    public async Task<Dictionary<int, Employee>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0)
            return new Dictionary<int, Employee>();

        var list = await _context.Employees
            .Include(e => e.Department)
            .AsNoTracking()
            .Where(e => idList.Contains(e.Id))
            .ToListAsync(cancellationToken);

        return list.ToDictionary(e => e.Id);
    }

    public async Task<Dictionary<int, string>> GetRolesByUserIdsAsync(IEnumerable<int> userIds, CancellationToken cancellationToken = default)
    {
        var idList = userIds.Distinct().ToList();
        if (idList.Count == 0)
            return new Dictionary<int, string>();

        var userRoles = await (
            from ur in _context.UserRoles
            join r in _context.Roles on ur.RoleId equals r.Id
            where idList.Contains(ur.UserId)
            select new { ur.UserId, RoleName = r.Name }
        ).ToListAsync(cancellationToken);

        return userRoles
            .GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => g.First().RoleName ?? string.Empty);
    }

    public async Task AddAsync(Employee employee, CancellationToken cancellationToken = default) =>
        await _context.Employees.AddAsync(employee, cancellationToken);

    public void Update(Employee employee) => _context.Employees.Update(employee);

    private static IQueryable<Employee> ApplySearch(IQueryable<Employee> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return query;

        var term = search.Trim().ToLower();
        return query.Where(e =>
            e.FirstName.ToLower().Contains(term) ||
            e.LastName.ToLower().Contains(term) ||
            (e.Email != null && e.Email.ToLower().Contains(term)));
    }
}
