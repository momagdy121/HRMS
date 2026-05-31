using HRSystem.Data.Common;
using HRSystem.Data.Context;
using HRSystem.Data.Interfaces;
using HRSystem.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.Data.Repositories;

public class EmployeeTaskRepository : IEmployeeTaskRepository
{
    private readonly AppDbContext _context;

    public EmployeeTaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<EmployeeTask?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.EmployeeTasks.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public Task<PagedList<EmployeeTask>> GetByAssignedByPagedAsync(int assignedById, int page, int pageSize, CancellationToken cancellationToken = default) =>
        _context.EmployeeTasks
            .AsNoTracking()
            .Where(t => !t.IsDeleted && t.AssignedById == assignedById)
            .OrderByDescending(t => t.CreatedAt)
            .ToPagedListAsync(page, pageSize, cancellationToken);

    public Task<PagedList<EmployeeTask>> GetByAssignedToPagedAsync(int assignedToId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        _context.EmployeeTasks
            .AsNoTracking()
            .Where(t => !t.IsDeleted && t.AssignedToId == assignedToId)
            .OrderByDescending(t => t.CreatedAt)
            .ToPagedListAsync(page, pageSize, cancellationToken);

    public Task<int> SoftDeleteAllForEmployeeAsync(int employeeId, CancellationToken cancellationToken = default) =>
        _context.EmployeeTasks
            .Where(t => !t.IsDeleted && (t.AssignedById == employeeId || t.AssignedToId == employeeId))
            .ExecuteUpdateAsync(
                s => s.SetProperty(t => t.IsDeleted, true),
                cancellationToken);

    public async Task AddAsync(EmployeeTask task, CancellationToken cancellationToken = default) =>
        await _context.EmployeeTasks.AddAsync(task, cancellationToken);

    public void Update(EmployeeTask task) => _context.EmployeeTasks.Update(task);
}
