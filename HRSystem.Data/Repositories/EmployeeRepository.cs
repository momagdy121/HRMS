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
        _context.Employees.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public Task<Employee?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        _context.Employees.FirstOrDefaultAsync(
            e => e.Email.ToLower() == email.ToLower(),
            cancellationToken);

    public Task<PagedList<Employee>> GetActivePagedAsync(int page, int pageSize, CancellationToken cancellationToken = default) =>
        _context.Employees
            .AsNoTracking()
            .Where(e => !e.IsDeleted)
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToPagedListAsync(page, pageSize, cancellationToken);

    public Task<PagedList<Employee>> GetByDepartmentPagedAsync(int departmentId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        _context.Employees
            .AsNoTracking()
            .Where(e => !e.IsDeleted && e.DepartmentId == departmentId)
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToPagedListAsync(page, pageSize, cancellationToken);

    public Task<PagedList<Employee>> GetDeletedPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default) =>
        _context.Employees
            .AsNoTracking()
            .Where(e => e.IsDeleted)
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToPagedListAsync(page, pageSize, cancellationToken);

    public Task<bool> IsManagerOfAnyDepartmentAsync(int employeeId, CancellationToken cancellationToken = default) =>
        _context.Departments.AnyAsync(d => d.ManagerId == employeeId && !d.IsDeleted, cancellationToken);

    public Task<bool> EmailExistsAsync(string email, int? excludeEmployeeId = null, CancellationToken cancellationToken = default)
    {
        var normalized = email.ToLower();
        var query = _context.Employees.Where(e => e.Email.ToLower() == normalized);
        if (excludeEmployeeId.HasValue)
            query = query.Where(e => e.Id != excludeEmployeeId.Value);

        return query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(Employee employee, CancellationToken cancellationToken = default) =>
        await _context.Employees.AddAsync(employee, cancellationToken);

    public void Update(Employee employee) => _context.Employees.Update(employee);
}
