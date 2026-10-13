using HRSystem.Common.Enums;
using HRSystem.Data.Common;
using HRSystem.Data.Context;
using HRSystem.Data.Interfaces;
using HRSystem.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.Data.Repositories;

public class JobApplicationRepository : IJobApplicationRepository
{
    private readonly AppDbContext _context;

    public JobApplicationRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<JobApplication?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.JobApplications
            .Include(a => a.JobPosting)
                .ThenInclude(j => j.Department)
            .Include(a => a.ReviewedBy)
            .Include(a => a.CreatedEmployee)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public Task<PagedList<JobApplication>> GetPagedAsync(
        int? jobPostingId,
        JobApplicationStatus? status,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.JobApplications
            .AsNoTracking()
            .Include(a => a.JobPosting)
                .ThenInclude(j => j.Department)
            .Include(a => a.ReviewedBy)
            .Include(a => a.CreatedEmployee)
            .AsQueryable();

        if (jobPostingId.HasValue && jobPostingId.Value > 0)
        {
            query = query.Where(a => a.JobPostingId == jobPostingId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(a =>
                a.FullName.Contains(term) ||
                a.Email.Contains(term) ||
                a.PhoneNumber.Contains(term) ||
                a.JobPosting.Title.Contains(term));
        }

        return query
            .OrderByDescending(a => a.SubmittedAt)
            .ToPagedListAsync(page, pageSize, cancellationToken);
    }

    public Task<bool> ExistsAsync(int jobPostingId, string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToUpperInvariant();
        return _context.JobApplications
            .AnyAsync(a => a.JobPostingId == jobPostingId && a.Email.ToUpper() == normalized, cancellationToken);
    }

    public Task<int> GetCountByJobPostingAsync(int jobPostingId, CancellationToken cancellationToken = default) =>
        _context.JobApplications.CountAsync(a => a.JobPostingId == jobPostingId, cancellationToken);

    public async Task AddAsync(JobApplication application, CancellationToken cancellationToken = default) =>
        await _context.JobApplications.AddAsync(application, cancellationToken);

    public void Update(JobApplication application) => _context.JobApplications.Update(application);
}
