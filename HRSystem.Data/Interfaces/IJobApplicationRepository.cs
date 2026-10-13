using HRSystem.Common.Enums;
using HRSystem.Data.Common;
using HRSystem.Data.Models;

namespace HRSystem.Data.Interfaces;

public interface IJobApplicationRepository
{
    Task<JobApplication?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<PagedList<JobApplication>> GetPagedAsync(
        int? jobPostingId,
        JobApplicationStatus? status,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(int jobPostingId, string email, CancellationToken cancellationToken = default);

    Task<int> GetCountByJobPostingAsync(int jobPostingId, CancellationToken cancellationToken = default);

    Task AddAsync(JobApplication application, CancellationToken cancellationToken = default);

    void Update(JobApplication application);
}
