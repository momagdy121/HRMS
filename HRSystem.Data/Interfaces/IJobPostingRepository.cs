using HRSystem.Common.Enums;
using HRSystem.Data.Common;
using HRSystem.Data.Models;

namespace HRSystem.Data.Interfaces;

public interface IJobPostingRepository
{
    Task<JobPosting?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<JobPosting?> GetByIdWithApplicationsAsync(int id, CancellationToken cancellationToken = default);

    Task<JobPosting?> GetPublishedActiveByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<PagedList<JobPosting>> GetAllPagedAsync(
        JobPostingStatus? status,
        int? departmentId,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<PagedList<JobPosting>> GetPublishedActivePagedAsync(
        int? departmentId,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task AddAsync(JobPosting posting, CancellationToken cancellationToken = default);

    void Update(JobPosting posting);
}
