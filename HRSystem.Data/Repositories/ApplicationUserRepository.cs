using HRSystem.Data.Common;
using HRSystem.Data.Context;
using HRSystem.Data.Interfaces;
using HRSystem.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.Data.Repositories;

public class ApplicationUserRepository : IApplicationUserRepository
{
    private readonly AppDbContext _context;

    public ApplicationUserRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<ApplicationUser?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<ApplicationUser?> GetByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default) =>
        _context.Users.FirstOrDefaultAsync(u => u.EmployeeId == employeeId, cancellationToken);

    public async Task<PagedList<UserAccountRow>> GetAllWithEmployeePagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query =
            from user in _context.Users.AsNoTracking()
            join employee in _context.Employees.AsNoTracking() on user.EmployeeId equals employee.Id
            orderby employee.LastName, employee.FirstName
            select new UserAccountRow { User = user, Employee = employee };

        return await query.ToPagedListAsync(page, pageSize, cancellationToken);
    }
}
