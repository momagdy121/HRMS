using HRSystem.Common.Enums;
using HRSystem.Data.Common;
using HRSystem.Data.Context;
using HRSystem.Data.Interfaces;
using HRSystem.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.Data.Repositories;

public class JobPostingRepository : IJobPostingRepository
{
    private readonly AppDbContext _context;

    public JobPostingRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<JobPosting?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.JobPostings
            .Include(j => j.Department)
            .Include(j => j.CreatedBy)
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);

    public Task<JobPosting?> GetByIdWithApplicationsAsync(int id, CancellationToken cancellationToken = default) =>
        _context.JobPostings
            .Include(j => j.Department)
            .Include(j => j.CreatedBy)
            .Include(j => j.Applications)
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);

    public Task<JobPosting?> GetPublishedActiveByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return _context.JobPostings
            .Include(j => j.Department)
            .FirstOrDefaultAsync(j => j.Id == id && j.Status == JobPostingStatus.Published && j.ClosingDate >= today, cancellationToken);
    }

    public Task<PagedList<JobPosting>> GetAllPagedAsync(
        JobPostingStatus? status,
        int? departmentId,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.JobPostings
            .AsNoTracking()
            .Include(j => j.Department)
            .Include(j => j.CreatedBy)
            .Include(j => j.Applications)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(j => j.Status == status.Value);
        }

        if (departmentId.HasValue && departmentId.Value > 0)
        {
            query = query.Where(j => j.DepartmentId == departmentId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(j => j.Title.Contains(term) || j.Department.Name.Contains(term));
        }

        return query
            .OrderByDescending(j => j.CreatedAt)
            .ToPagedListAsync(page, pageSize, cancellationToken);
    }

    public Task<PagedList<JobPosting>> GetPublishedActivePagedAsync(
        int? departmentId,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var query = _context.JobPostings
            .AsNoTracking()
            .Include(j => j.Department)
            .Where(j => j.Status == JobPostingStatus.Published && j.ClosingDate >= today);

        if (departmentId.HasValue && departmentId.Value > 0)
        {
            query = query.Where(j => j.DepartmentId == departmentId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(j => j.Title.Contains(term) || j.Department.Name.Contains(term));
        }

        return query
            .OrderBy(j => j.ClosingDate)
            .ThenByDescending(j => j.PublishedAt ?? j.CreatedAt)
            .ToPagedListAsync(page, pageSize, cancellationToken);
    }

    public async Task AddAsync(JobPosting posting, CancellationToken cancellationToken = default) =>
        await _context.JobPostings.AddAsync(posting, cancellationToken);

    public void Update(JobPosting posting) => _context.JobPostings.Update(posting);
}
