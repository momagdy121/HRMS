using HRSystem.Data.Common;
using HRSystem.Data.Context;
using HRSystem.Data.Interfaces;
using HRSystem.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.Data.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly AppDbContext _context;

    public DepartmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Department?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Departments.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public Task<Department?> GetByManagerIdAsync(int managerId, CancellationToken cancellationToken = default) =>
        _context.Departments.FirstOrDefaultAsync(
            d => d.ManagerId == managerId && !d.IsDeleted,
            cancellationToken);

    public Task<PagedList<Department>> GetActivePagedAsync(int page, int pageSize, CancellationToken cancellationToken = default) =>
        _context.Departments
            .AsNoTracking()
            .Where(d => !d.IsDeleted)
            .OrderBy(d => d.Name)
            .ToPagedListAsync(page, pageSize, cancellationToken);

    public Task<PagedList<Department>> GetDeletedPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default) =>
        _context.Departments
            .AsNoTracking()
            .Where(d => d.IsDeleted)
            .OrderBy(d => d.Name)
            .ToPagedListAsync(page, pageSize, cancellationToken);

    public Task<int> CountActiveEmployeesAsync(int departmentId, CancellationToken cancellationToken = default) =>
        _context.Employees.CountAsync(
            e => e.DepartmentId == departmentId && !e.IsDeleted,
            cancellationToken);

    public async Task AddAsync(Department department, CancellationToken cancellationToken = default) =>
        await _context.Departments.AddAsync(department, cancellationToken);

    public void Update(Department department) => _context.Departments.Update(department);
}
