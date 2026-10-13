using HRSystem.Business.DTOs.Employees;
using HRSystem.Business.DTOs.Recruitment;
using HRSystem.Business.Exceptions;
using HRSystem.Business.Helpers;
using HRSystem.Business.Interfaces.Services;
using HRSystem.Common.Constants;
using HRSystem.Common.Enums;
using HRSystem.Data.Common;
using HRSystem.Data.Interfaces;
using HRSystem.Data.Models;
using Microsoft.Extensions.Logging;

namespace HRSystem.Business.Services;

public class RecruitmentService : IRecruitmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly IEmailService _emailService;
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<RecruitmentService> _logger;

    public RecruitmentService(
        IUnitOfWork unitOfWork,
        ICloudinaryService cloudinaryService,
        IEmailService emailService,
        IEmployeeService employeeService,
        ILogger<RecruitmentService> logger)
    {
        _unitOfWork = unitOfWork;
        _cloudinaryService = cloudinaryService;
        _emailService = emailService;
        _employeeService = employeeService;
        _logger = logger;
    }

    // ── Job Postings Management (CEO) ──────────────────────────────
    public async Task<JobPosting> GetJobPostingByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await _unitOfWork.JobPostings.GetByIdAsync(id, cancellationToken)
        ?? throw new NotFoundException($"Job posting with ID {id} was not found.");

    public Task<PagedList<JobPosting>> GetJobPostingsPagedAsync(
        JobPostingStatus? status,
        int? departmentId,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default) =>
        _unitOfWork.JobPostings.GetAllPagedAsync(status, departmentId, search, page, pageSize, cancellationToken);

    public async Task<JobPosting> CreateJobPostingAsync(CreateJobPostingDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new BusinessRuleException("Job title is required.");

        if (string.IsNullOrWhiteSpace(dto.Description))
            throw new BusinessRuleException("Job description is required.");

        if (string.IsNullOrWhiteSpace(dto.Requirements))
            throw new BusinessRuleException("Job requirements are required.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (dto.ClosingDate < today)
            throw new BusinessRuleException("Application closing date cannot be in the past.");

        var department = await _unitOfWork.Departments.GetByIdAsync(dto.DepartmentId, cancellationToken)
                         ?? throw new NotFoundException($"Department with ID {dto.DepartmentId} not found.");

        var posting = new JobPosting
        {
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            Requirements = dto.Requirements.Trim(),
            DepartmentId = dto.DepartmentId,
            ClosingDate = dto.ClosingDate,
            Status = JobPostingStatus.Draft,
            CreatedById = dto.CreatedById,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.JobPostings.AddAsync(posting, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return posting;
    }

    public async Task<JobPosting> UpdateJobPostingAsync(UpdateJobPostingDto dto, CancellationToken cancellationToken = default)
    {
        var posting = await _unitOfWork.JobPostings.GetByIdAsync(dto.Id, cancellationToken)
                      ?? throw new NotFoundException($"Job posting with ID {dto.Id} was not found.");

        if (posting.Status == JobPostingStatus.Closed)
            throw new BusinessRuleException("Closed job postings cannot be modified.");

        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new BusinessRuleException("Job title is required.");

        if (string.IsNullOrWhiteSpace(dto.Description))
            throw new BusinessRuleException("Job description is required.");

        if (string.IsNullOrWhiteSpace(dto.Requirements))
            throw new BusinessRuleException("Job requirements are required.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (dto.ClosingDate < today)
            throw new BusinessRuleException("Application closing date cannot be in the past.");

        var department = await _unitOfWork.Departments.GetByIdAsync(dto.DepartmentId, cancellationToken)
                         ?? throw new NotFoundException($"Department with ID {dto.DepartmentId} not found.");

        posting.Title = dto.Title.Trim();
        posting.Description = dto.Description.Trim();
        posting.Requirements = dto.Requirements.Trim();
        posting.DepartmentId = dto.DepartmentId;
        posting.ClosingDate = dto.ClosingDate;

        _unitOfWork.JobPostings.Update(posting);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return posting;
    }

    public async Task<JobPosting> PublishJobPostingAsync(int id, CancellationToken cancellationToken = default)
    {
        var posting = await _unitOfWork.JobPostings.GetByIdAsync(id, cancellationToken)
                      ?? throw new NotFoundException($"Job posting with ID {id} was not found.");

        if (posting.Status == JobPostingStatus.Closed)
            throw new BusinessRuleException("Closed job postings cannot be published.");

        if (posting.Status == JobPostingStatus.Published)
            throw new BusinessRuleException("This job posting is already published.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (posting.ClosingDate < today)
            throw new BusinessRuleException("Cannot publish a job posting with an expired closing date.");

        posting.Status = JobPostingStatus.Published;
        posting.PublishedAt = DateTime.UtcNow;

        _unitOfWork.JobPostings.Update(posting);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return posting;
    }

    public async Task<JobPosting> CloseJobPostingAsync(int id, CancellationToken cancellationToken = default)
    {
        var posting = await _unitOfWork.JobPostings.GetByIdAsync(id, cancellationToken)
                      ?? throw new NotFoundException($"Job posting with ID {id} was not found.");

        if (posting.Status == JobPostingStatus.Closed)
            throw new BusinessRuleException("This job posting is already closed.");

        posting.Status = JobPostingStatus.Closed;
        posting.ClosedAt = DateTime.UtcNow;

        _unitOfWork.JobPostings.Update(posting);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return posting;
    }

    // ── Public Careers Portal ──────────────────────────────────────
    public Task<PagedList<JobPosting>> GetActivePublicJobPostingsPagedAsync(
        int? departmentId,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default) =>
        _unitOfWork.JobPostings.GetPublishedActivePagedAsync(departmentId, search, page, pageSize, cancellationToken);

    public Task<JobPosting?> GetPublicJobPostingDetailsAsync(int id, CancellationToken cancellationToken = default) =>
        _unitOfWork.JobPostings.GetPublishedActiveByIdAsync(id, cancellationToken);

    public async Task<JobApplication> SubmitApplicationAsync(SubmitJobApplicationDto dto, CancellationToken cancellationToken = default)
    {
        var posting = await _unitOfWork.JobPostings.GetByIdAsync(dto.JobPostingId, cancellationToken)
                      ?? throw new NotFoundException("The selected job posting does not exist.");

        if (posting.Status != JobPostingStatus.Published)
            throw new BusinessRuleException("This job posting is not currently accepting applications.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (posting.ClosingDate < today)
            throw new BusinessRuleException("The application closing deadline for this job posting has passed.");

        if (string.IsNullOrWhiteSpace(dto.FullName))
            throw new BusinessRuleException("Full name is required.");

        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new BusinessRuleException("Email address is required.");

        if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
            throw new BusinessRuleException("Phone number is required.");

        var normalizedEmail = dto.Email.Trim();
        if (await _unitOfWork.JobApplications.ExistsAsync(dto.JobPostingId, normalizedEmail, cancellationToken))
            throw new BusinessRuleException("An application with this email address has already been submitted for this position.");

        var uploadResult = await _cloudinaryService.UploadDocumentAsync(dto.CvFile, "hrms/cvs", cancellationToken);
        if (!uploadResult.Succeeded)
            throw new BusinessRuleException(uploadResult.ErrorMessage ?? "CV upload failed.");

        var application = new JobApplication
        {
            JobPostingId = dto.JobPostingId,
            FullName = dto.FullName.Trim(),
            Email = normalizedEmail,
            PhoneNumber = dto.PhoneNumber.Trim(),
            CoverLetter = dto.CoverLetter?.Trim(),
            CvPublicId = uploadResult.PublicId,
            CvOriginalFileName = uploadResult.OriginalFileName,
            CvStorageUrl = uploadResult.StorageUrl,
            CvContentType = uploadResult.ContentType,
            CvFileSize = uploadResult.FileSize,
            Status = JobApplicationStatus.Submitted,
            SubmittedAt = DateTime.UtcNow
        };

        try
        {
            await _unitOfWork.JobApplications.AddAsync(application, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return application;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save application to database. Cleaning up uploaded CV asset {PublicId}.", uploadResult.PublicId);
            try
            {
                await _cloudinaryService.DeleteAsync(uploadResult.PublicId, cancellationToken);
            }
            catch (Exception cleanupEx)
            {
                _logger.LogWarning(cleanupEx, "Failed to clean up uploaded CV file after database failure.");
            }
            throw;
        }
    }

    // ── Applications Management (CEO) ──────────────────────────────
    public async Task<JobApplication> GetApplicationByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await _unitOfWork.JobApplications.GetByIdAsync(id, cancellationToken)
        ?? throw new NotFoundException($"Job application with ID {id} was not found.");

    public Task<PagedList<JobApplication>> GetApplicationsPagedAsync(
        int? jobPostingId,
        JobApplicationStatus? status,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default) =>
        _unitOfWork.JobApplications.GetPagedAsync(jobPostingId, status, search, page, pageSize, cancellationToken);

    public async Task UpdateReviewNotesAsync(int applicationId, string? notes, int reviewerId, CancellationToken cancellationToken = default)
    {
        var application = await _unitOfWork.JobApplications.GetByIdAsync(applicationId, cancellationToken)
                          ?? throw new NotFoundException($"Job application with ID {applicationId} was not found.");

        application.ReviewNotes = notes?.Trim();
        application.ReviewedById = reviewerId;
        application.ReviewedAt = DateTime.UtcNow;

        _unitOfWork.JobApplications.Update(application);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<JobApplication> AcceptApplicationAsync(int applicationId, int reviewerId, CancellationToken cancellationToken = default)
    {
        var application = await _unitOfWork.JobApplications.GetByIdAsync(applicationId, cancellationToken)
                          ?? throw new NotFoundException($"Job application with ID {applicationId} was not found.");

        if (application.Status != JobApplicationStatus.Submitted)
            throw new BusinessRuleException($"Cannot accept an application that is already '{application.Status}'. Only submitted applications can be reviewed.");

        application.Status = JobApplicationStatus.Accepted;
        application.ReviewedById = reviewerId;
        application.ReviewedAt = DateTime.UtcNow;

        _unitOfWork.JobApplications.Update(application);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return application;
    }

    public async Task<JobApplication> RejectApplicationAsync(int applicationId, int reviewerId, CancellationToken cancellationToken = default)
    {
        var application = await _unitOfWork.JobApplications.GetByIdAsync(applicationId, cancellationToken)
                          ?? throw new NotFoundException($"Job application with ID {applicationId} was not found.");

        if (application.Status != JobApplicationStatus.Submitted)
            throw new BusinessRuleException($"Cannot reject an application that is already '{application.Status}'. Only submitted applications can be reviewed.");

        application.Status = JobApplicationStatus.Rejected;
        application.ReviewedById = reviewerId;
        application.ReviewedAt = DateTime.UtcNow;

        _unitOfWork.JobApplications.Update(application);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send rejection email after DB operation has succeeded
        await TrySendRejectionEmailAsync(application, cancellationToken);

        return application;
    }

    public async Task<bool> RetryRejectionEmailAsync(int applicationId, CancellationToken cancellationToken = default)
    {
        var application = await _unitOfWork.JobApplications.GetByIdAsync(applicationId, cancellationToken)
                          ?? throw new NotFoundException($"Job application with ID {applicationId} was not found.");

        if (application.Status != JobApplicationStatus.Rejected)
            throw new BusinessRuleException("Rejection email can only be sent for rejected applications.");

        return await TrySendRejectionEmailAsync(application, cancellationToken);
    }

    private async Task<bool> TrySendRejectionEmailAsync(JobApplication application, CancellationToken cancellationToken)
    {
        try
        {
            var jobTitle = application.JobPosting?.Title ?? "Position";
            var emailBody = EmailTemplates.ApplicationRejectionEmail(application.FullName, jobTitle);

            await _emailService.SendEmailAsync(
                application.Email,
                application.FullName,
                $"Application Update - {jobTitle}",
                emailBody,
                cancellationToken);

            application.RejectionEmailSent = true;
            application.RejectionEmailSentAt = DateTime.UtcNow;
            _unitOfWork.JobApplications.Update(application);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send rejection email to candidate {Email} for application {ApplicationId}", application.Email, application.Id);
            return false;
        }
    }

    public async Task<(Stream Stream, string ContentType, string FileName)?> GetCvDocumentStreamAsync(int applicationId, CancellationToken cancellationToken = default)
    {
        var application = await _unitOfWork.JobApplications.GetByIdAsync(applicationId, cancellationToken)
                          ?? throw new NotFoundException($"Job application with ID {applicationId} was not found.");

        return await _cloudinaryService.GetDocumentStreamAsync(
            application.CvPublicId,
            application.CvStorageUrl,
            application.CvOriginalFileName,
            application.CvContentType,
            cancellationToken);
    }

    // ── Employee Onboarding ────────────────────────────────────────
    public async Task<Employee> OnboardAcceptedCandidateAsync(OnboardCandidateDto dto, CancellationToken cancellationToken = default)
    {
        var application = await _unitOfWork.JobApplications.GetByIdAsync(dto.ApplicationId, cancellationToken)
                          ?? throw new NotFoundException($"Job application with ID {dto.ApplicationId} was not found.");

        if (application.Status != JobApplicationStatus.Accepted)
            throw new BusinessRuleException("Candidate application must be accepted before creating an employee account.");

        if (application.CreatedEmployeeId.HasValue)
            throw new BusinessRuleException("An employee account has already been created for this application.");

        if (string.Equals(dto.Role, RoleNames.CEO, StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleException("Cannot hire or create an employee with the CEO role.");

        if (await _unitOfWork.Employees.EmailExistsAsync(dto.Email, cancellationToken: cancellationToken))
            throw new BusinessRuleException("An employee account with this email address already exists.");

        // Reuse existing Employee creation workflow (creates Identity account, initializes leave balances, sets IsPasswordChangeRequired = true)
        var createDto = new CreateEmployeeDto
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = dto.Email.Trim(),
            DepartmentId = dto.DepartmentId,
            Salary = dto.Salary,
            HireDate = dto.HireDate,
            Role = dto.Role,
            InitialPassword = dto.InitialPassword
        };

        var employee = await _employeeService.CreateAsync(createDto, cancellationToken);

        // Associate created Employee with the accepted application
        application.CreatedEmployeeId = employee.Id;
        _unitOfWork.JobApplications.Update(application);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send onboarding acceptance email with credentials and first-login notice
        await TrySendOnboardingEmailAsync(application, dto.FirstName, dto.LastName, dto.Email, dto.InitialPassword, dto.LoginUrl, cancellationToken);

        return employee;
    }

    public async Task<bool> RetryOnboardingEmailAsync(int applicationId, string temporaryPassword, string? loginUrl, CancellationToken cancellationToken = default)
    {
        var application = await _unitOfWork.JobApplications.GetByIdAsync(applicationId, cancellationToken)
                          ?? throw new NotFoundException($"Job application with ID {applicationId} was not found.");

        if (!application.CreatedEmployeeId.HasValue)
            throw new BusinessRuleException("No employee account exists for this application. Please create the employee account first.");

        if (string.IsNullOrWhiteSpace(temporaryPassword))
            throw new BusinessRuleException("Temporary password is required to send onboarding credentials.");

        var employee = await _unitOfWork.Employees.GetByIdAsync(application.CreatedEmployeeId.Value, cancellationToken)
                       ?? throw new NotFoundException("Employee record not found.");

        return await TrySendOnboardingEmailAsync(application, employee.FirstName, employee.LastName, employee.Email ?? application.Email, temporaryPassword, loginUrl, cancellationToken);
    }

    private async Task<bool> TrySendOnboardingEmailAsync(
        JobApplication application,
        string firstName,
        string lastName,
        string email,
        string temporaryPassword,
        string? loginUrl,
        CancellationToken cancellationToken)
    {
        try
        {
            var jobTitle = application.JobPosting?.Title ?? "Position";
            var candidateName = $"{firstName} {lastName}".Trim();
            if (string.IsNullOrEmpty(candidateName)) candidateName = application.FullName;

            var portalLoginUrl = !string.IsNullOrWhiteSpace(loginUrl) ? loginUrl : "/Account/Login";
            var emailBody = EmailTemplates.ApplicantAcceptedOnboardingEmail(
                candidateName,
                jobTitle,
                email,
                temporaryPassword,
                portalLoginUrl);

            await _emailService.SendEmailAsync(
                email,
                candidateName,
                $"Welcome to the Team - Your HRMS Account Credentials",
                emailBody,
                cancellationToken);

            application.AcceptanceEmailSent = true;
            application.AcceptanceEmailSentAt = DateTime.UtcNow;
            _unitOfWork.JobApplications.Update(application);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send onboarding credentials email to candidate {Email} for application {ApplicationId}", email, application.Id);
            return false;
        }
    }
}
