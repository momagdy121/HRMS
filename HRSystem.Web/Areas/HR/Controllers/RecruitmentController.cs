using HRSystem.Business.DTOs.Recruitment;
using HRSystem.Business.Exceptions;
using HRSystem.Business.Interfaces.Services;
using HRSystem.Common.Constants;
using HRSystem.Common.Enums;
using HRSystem.Data.Interfaces;
using HRSystem.Web.ViewModels.Recruitment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRSystem.Web.Areas.HR.Controllers;

public class RecruitmentController : HRBaseController
{
    private const int PageSize = 10;
    private readonly IRecruitmentService _recruitmentService;
    private readonly ILogger<RecruitmentController> _logger;

    public RecruitmentController(
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork,
        IRecruitmentService recruitmentService,
        ILogger<RecruitmentController> logger)
        : base(currentUser, unitOfWork)
    {
        _recruitmentService = recruitmentService;
        _logger = logger;
    }

    // ── Job Postings Management ──────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Index(
        int page = 1,
        JobPostingStatus? status = null,
        int? departmentId = null,
        string? search = null)
    {
        await SetLayoutAsync("Recruitment", "Search job postings...");

        var postings = await _recruitmentService.GetJobPostingsPagedAsync(
            status,
            departmentId,
            search,
            page,
            PageSize);

        var departments = (await UnitOfWork.Departments.GetActivePagedAsync(1, 1000))
            .Items
            .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name })
            .ToList();

        var viewModel = new JobPostingListViewModel
        {
            Postings = postings,
            Status = status,
            DepartmentId = departmentId,
            Search = search,
            Departments = departments
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await SetLayoutAsync("Recruitment", "Create job posting...");

        var model = new JobPostingFormViewModel
        {
            Departments = await GetDepartmentSelectListAsync()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(JobPostingFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await SetLayoutAsync("Recruitment", "Create job posting...");
            model.Departments = await GetDepartmentSelectListAsync();
            return View(model);
        }

        try
        {
            var creatorId = CurrentUser.GetCurrentUserId();
            await _recruitmentService.CreateJobPostingAsync(new CreateJobPostingDto
            {
                Title = model.Title,
                DepartmentId = model.DepartmentId,
                Description = model.Description,
                Requirements = model.Requirements,
                ClosingDate = model.ClosingDate,
                CreatedById = creatorId
            });

            TempData["Success"] = "Job opening created successfully as Draft.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleException ex)
        {
            await SetLayoutAsync("Recruitment", "Create job posting...");
            ModelState.AddModelError(string.Empty, ex.Message);
            model.Departments = await GetDepartmentSelectListAsync();
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        await SetLayoutAsync("Recruitment", "Edit job posting...");

        try
        {
            var posting = await _recruitmentService.GetJobPostingByIdAsync(id);
            if (posting.Status == JobPostingStatus.Closed)
            {
                TempData["Error"] = "Closed job postings cannot be edited.";
                return RedirectToAction(nameof(Index));
            }

            var model = new JobPostingFormViewModel
            {
                Id = posting.Id,
                Title = posting.Title,
                DepartmentId = posting.DepartmentId,
                Description = posting.Description,
                Requirements = posting.Requirements,
                ClosingDate = posting.ClosingDate,
                Departments = await GetDepartmentSelectListAsync()
            };

            return View(model);
        }
        catch (NotFoundException)
        {
            TempData["Error"] = "Job posting not found.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, JobPostingFormViewModel model)
    {
        if (id != model.Id)
        {
            TempData["Error"] = "Invalid request.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            await SetLayoutAsync("Recruitment", "Edit job posting...");
            model.Departments = await GetDepartmentSelectListAsync();
            return View(model);
        }

        try
        {
            await _recruitmentService.UpdateJobPostingAsync(new UpdateJobPostingDto
            {
                Id = model.Id,
                Title = model.Title,
                DepartmentId = model.DepartmentId,
                Description = model.Description,
                Requirements = model.Requirements,
                ClosingDate = model.ClosingDate
            });

            TempData["Success"] = "Job opening updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleException ex)
        {
            await SetLayoutAsync("Recruitment", "Edit job posting...");
            ModelState.AddModelError(string.Empty, ex.Message);
            model.Departments = await GetDepartmentSelectListAsync();
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(int id)
    {
        try
        {
            await _recruitmentService.PublishJobPostingAsync(id);
            TempData["Success"] = "Job opening published successfully! Candidates can now apply from the Careers page.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(int id)
    {
        try
        {
            await _recruitmentService.CloseJobPostingAsync(id);
            TempData["Success"] = "Job opening has been closed. It will no longer accept new applications.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    // ── Candidate Applications Management ────────────────────────
    [HttpGet]
    public async Task<IActionResult> Applications(
        int page = 1,
        int? jobPostingId = null,
        JobApplicationStatus? status = null,
        string? search = null)
    {
        await SetLayoutAsync("Recruitment", "Search applications...");

        var applications = await _recruitmentService.GetApplicationsPagedAsync(
            jobPostingId,
            status,
            search,
            page,
            PageSize);

        var allPostings = (await UnitOfWork.JobPostings.GetAllPagedAsync(null, null, null, 1, 1000)).Items
            .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Title })
            .ToList();

        var viewModel = new ApplicationListViewModel
        {
            Applications = applications,
            JobPostingId = jobPostingId,
            Status = status,
            Search = search,
            JobPostings = allPostings
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> ApplicationDetails(int id)
    {
        await SetLayoutAsync("Recruitment", "Application Details...");

        try
        {
            var application = await _recruitmentService.GetApplicationByIdAsync(id);
            return View(new ApplicationDetailsViewModel { Application = application });
        }
        catch (NotFoundException)
        {
            TempData["Error"] = "Job application not found.";
            return RedirectToAction(nameof(Applications));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateNotes(int id, string? notes)
    {
        try
        {
            var reviewerId = CurrentUser.GetCurrentUserId();
            await _recruitmentService.UpdateReviewNotesAsync(id, notes, reviewerId);
            TempData["Success"] = "Review notes updated.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(ApplicationDetails), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(int id)
    {
        try
        {
            var reviewerId = CurrentUser.GetCurrentUserId();
            await _recruitmentService.AcceptApplicationAsync(id, reviewerId);
            TempData["Success"] = "Candidate application accepted! You can now start the onboarding process.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(ApplicationDetails), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id)
    {
        try
        {
            var reviewerId = CurrentUser.GetCurrentUserId();
            var application = await _recruitmentService.RejectApplicationAsync(id, reviewerId);

            if (application.RejectionEmailSent)
            {
                TempData["Success"] = "Application rejected. Professional rejection email notification has been sent to candidate.";
            }
            else
            {
                TempData["Warning"] = "Application status changed to Rejected, but the rejection email notification failed to deliver. You can retry sending it.";
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(ApplicationDetails), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RetryRejectionEmail(int id)
    {
        try
        {
            var sent = await _recruitmentService.RetryRejectionEmailAsync(id);
            if (sent)
            {
                TempData["Success"] = "Rejection email sent successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to deliver rejection email. Please check email service configuration.";
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(ApplicationDetails), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> DownloadCv(int id)
    {
        try
        {
            var result = await _recruitmentService.GetCvDocumentStreamAsync(id);
            if (result == null)
            {
                TempData["Error"] = "Unable to retrieve the CV document.";
                return RedirectToAction(nameof(ApplicationDetails), new { id });
            }

            return File(result.Value.Stream, result.Value.ContentType, result.Value.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while downloading CV document for application {Id}", id);
            TempData["Error"] = "Could not download document.";
            return RedirectToAction(nameof(ApplicationDetails), new { id });
        }
    }

    // ── Employee Onboarding ────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Onboard(int id)
    {
        await SetLayoutAsync("Recruitment", "Onboard Candidate...");

        try
        {
            var application = await _recruitmentService.GetApplicationByIdAsync(id);
            if (application.Status != JobApplicationStatus.Accepted)
            {
                TempData["Error"] = "Candidate must be Accepted before onboarding.";
                return RedirectToAction(nameof(ApplicationDetails), new { id });
            }

            if (application.CreatedEmployeeId.HasValue)
            {
                TempData["Error"] = "An employee account has already been created for this candidate.";
                return RedirectToAction(nameof(ApplicationDetails), new { id });
            }

            // Split candidate full name into first and last
            var nameParts = application.FullName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var firstName = nameParts.Length > 0 ? nameParts[0] : application.FullName;
            var lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;

            var model = new OnboardCandidateViewModel
            {
                ApplicationId = application.Id,
                CandidateFullName = application.FullName,
                JobTitle = application.JobPosting.Title,
                FirstName = firstName,
                LastName = lastName,
                Email = application.Email,
                DepartmentId = application.JobPosting.DepartmentId,
                Salary = 0,
                HireDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Role = RoleNames.Employee,
                InitialPassword = GenerateInitialPassword(),
                Departments = await GetDepartmentSelectListAsync(),
                AllowedRoles = GetAllowedRolesSelectList()
            };

            return View(model);
        }
        catch (NotFoundException)
        {
            TempData["Error"] = "Application not found.";
            return RedirectToAction(nameof(Applications));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Onboard(int id, OnboardCandidateViewModel model)
    {
        if (id != model.ApplicationId)
        {
            TempData["Error"] = "Invalid request.";
            return RedirectToAction(nameof(Applications));
        }

        if (string.Equals(model.Role, RoleNames.CEO, StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(model.Role), "Cannot assign the CEO role to a new employee.");
        }

        if (!ModelState.IsValid)
        {
            await SetLayoutAsync("Recruitment", "Onboard Candidate...");
            model.Departments = await GetDepartmentSelectListAsync();
            model.AllowedRoles = GetAllowedRolesSelectList();
            return View(model);
        }

        try
        {
            var loginUrl = Url.Action("Login", "Account", new { area = string.Empty }, Request.Scheme)
                           ?? "/Account/Login";

            var employee = await _recruitmentService.OnboardAcceptedCandidateAsync(new OnboardCandidateDto
            {
                ApplicationId = model.ApplicationId,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                DepartmentId = model.DepartmentId,
                Salary = model.Salary,
                HireDate = model.HireDate,
                Role = model.Role,
                InitialPassword = model.InitialPassword,
                LoginUrl = loginUrl
            });

            TempData["Success"] = $"Employee account for {employee.FirstName} {employee.LastName} has been created successfully! Onboarding credentials sent via email.";
            return RedirectToAction(nameof(ApplicationDetails), new { id = model.ApplicationId });
        }
        catch (BusinessRuleException ex)
        {
            await SetLayoutAsync("Recruitment", "Onboard Candidate...");
            ModelState.AddModelError(string.Empty, ex.Message);
            model.Departments = await GetDepartmentSelectListAsync();
            model.AllowedRoles = GetAllowedRolesSelectList();
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to onboard candidate for application {Id}", id);
            await SetLayoutAsync("Recruitment", "Onboard Candidate...");
            ModelState.AddModelError(string.Empty, "An error occurred while creating the employee account. Please try again.");
            model.Departments = await GetDepartmentSelectListAsync();
            model.AllowedRoles = GetAllowedRolesSelectList();
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RetryOnboardingEmail(int id, string temporaryPassword)
    {
        try
        {
            var loginUrl = Url.Action("Login", "Account", new { area = string.Empty }, Request.Scheme)
                           ?? "/Account/Login";

            var sent = await _recruitmentService.RetryOnboardingEmailAsync(id, temporaryPassword, loginUrl);
            if (sent)
            {
                TempData["Success"] = "Onboarding email sent successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to send onboarding credentials email. Please verify email configuration.";
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(ApplicationDetails), new { id });
    }

    // ── Helpers ──────────────────────────────────────────────────
    private async Task<List<SelectListItem>> GetDepartmentSelectListAsync() =>
        (await UnitOfWork.Departments.GetActivePagedAsync(1, 1000))
            .Items
            .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name })
            .ToList();

    private static List<SelectListItem> GetAllowedRolesSelectList() =>
        new()
        {
            new SelectListItem { Value = RoleNames.Employee, Text = "Employee" },
            new SelectListItem { Value = RoleNames.DepartmentHead, Text = "Department Head" }
        };

    private static string GenerateInitialPassword() =>
        $"Welcome#{Random.Shared.Next(100000, 999999)}!";
}
