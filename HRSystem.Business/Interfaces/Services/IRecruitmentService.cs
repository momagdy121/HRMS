using HRSystem.Business.DTOs.Recruitment;
using HRSystem.Common.Enums;
using HRSystem.Data.Common;
using HRSystem.Data.Models;

namespace HRSystem.Business.Interfaces.Services;

public interface IRecruitmentService
{
    // ── Job Postings (CEO Management) ──────────────────────────────
    Task<JobPosting> GetJobPostingByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<PagedList<JobPosting>> GetJobPostingsPagedAsync(
        JobPostingStatus? status,
        int? departmentId,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<JobPosting> CreateJobPostingAsync(CreateJobPostingDto dto, CancellationToken cancellationToken = default);

    Task<JobPosting> UpdateJobPostingAsync(UpdateJobPostingDto dto, CancellationToken cancellationToken = default);

    Task<JobPosting> PublishJobPostingAsync(int id, CancellationToken cancellationToken = default);

    Task<JobPosting> CloseJobPostingAsync(int id, CancellationToken cancellationToken = default);

    // ── Public Careers Portal ──────────────────────────────────────
    Task<PagedList<JobPosting>> GetActivePublicJobPostingsPagedAsync(
        int? departmentId,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<JobPosting?> GetPublicJobPostingDetailsAsync(int id, CancellationToken cancellationToken = default);

    Task<JobApplication> SubmitApplicationAsync(SubmitJobApplicationDto dto, CancellationToken cancellationToken = default);

    // ── Applications Management (CEO) ──────────────────────────────
    Task<JobApplication> GetApplicationByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<PagedList<JobApplication>> GetApplicationsPagedAsync(
        int? jobPostingId,
        JobApplicationStatus? status,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task UpdateReviewNotesAsync(int applicationId, string? notes, int reviewerId, CancellationToken cancellationToken = default);

    Task<JobApplication> AcceptApplicationAsync(int applicationId, int reviewerId, CancellationToken cancellationToken = default);

    Task<JobApplication> RejectApplicationAsync(int applicationId, int reviewerId, CancellationToken cancellationToken = default);

    Task<bool> RetryRejectionEmailAsync(int applicationId, CancellationToken cancellationToken = default);

    Task<(Stream Stream, string ContentType, string FileName)?> GetCvDocumentStreamAsync(int applicationId, CancellationToken cancellationToken = default);

    // ── Employee Onboarding ────────────────────────────────────────
    Task<Employee> OnboardAcceptedCandidateAsync(OnboardCandidateDto dto, CancellationToken cancellationToken = default);

    Task<bool> RetryOnboardingEmailAsync(int applicationId, string temporaryPassword, string? loginUrl, CancellationToken cancellationToken = default);
}
