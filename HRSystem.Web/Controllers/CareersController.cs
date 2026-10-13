using HRSystem.Business.DTOs.Recruitment;
using HRSystem.Business.Exceptions;
using HRSystem.Business.Helpers;
using HRSystem.Business.Interfaces.Services;
using HRSystem.Data.Interfaces;
using HRSystem.Web.ViewModels.Recruitment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRSystem.Web.Controllers;

[AllowAnonymous]
public class CareersController : Controller
{
    private const int PageSize = 10;
    private readonly IRecruitmentService _recruitmentService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CareersController> _logger;

    public CareersController(
        IRecruitmentService recruitmentService,
        IUnitOfWork unitOfWork,
        ILogger<CareersController> logger)
    {
        _recruitmentService = recruitmentService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, int? departmentId = null, string? search = null)
    {
        ViewBag.HideShell = true;
        ViewBag.Title = "Careers & Open Positions - HRMS Portal";

        var postings = await _recruitmentService.GetActivePublicJobPostingsPagedAsync(
            departmentId,
            search,
            page,
            PageSize);

        var departments = (await _unitOfWork.Departments.GetActivePagedAsync(1, 1000))
            .Items
            .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name })
            .ToList();

        var viewModel = new CareersIndexViewModel
        {
            Postings = postings,
            DepartmentId = departmentId,
            Search = search,
            Departments = departments
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        ViewBag.HideShell = true;

        var posting = await _recruitmentService.GetPublicJobPostingDetailsAsync(id);
        if (posting == null)
        {
            TempData["Error"] = "The requested job opening could not be found or has closed.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Title = $"{posting.Title} - Careers";

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var isAccepting = posting.Status == Common.Enums.JobPostingStatus.Published && posting.ClosingDate >= today;
        var daysRemaining = Math.Max(0, posting.ClosingDate.DayNumber - today.DayNumber);

        var viewModel = new JobDetailsViewModel
        {
            Posting = posting,
            IsAcceptingApplications = isAccepting,
            DaysRemaining = daysRemaining
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Apply(int id)
    {
        ViewBag.HideShell = true;

        var posting = await _recruitmentService.GetPublicJobPostingDetailsAsync(id);
        if (posting == null)
        {
            TempData["Error"] = "The requested position is currently closed or not accepting applications.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Title = $"Apply for {posting.Title} - Careers";

        var model = new ApplyJobViewModel
        {
            JobPostingId = posting.Id,
            JobTitle = posting.Title,
            DepartmentName = posting.Department?.Name ?? "General",
            ClosingDate = posting.ClosingDate
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(int id, ApplyJobViewModel model)
    {
        ViewBag.HideShell = true;

        var posting = await _recruitmentService.GetPublicJobPostingDetailsAsync(id);
        if (posting == null)
        {
            TempData["Error"] = "This position is no longer accepting applications.";
            return RedirectToAction(nameof(Index));
        }

        model.JobPostingId = posting.Id;
        model.JobTitle = posting.Title;
        model.DepartmentName = posting.Department?.Name ?? "General";
        model.ClosingDate = posting.ClosingDate;
        ViewBag.Title = $"Apply for {posting.Title} - Careers";

        if (model.CvFile == null || model.CvFile.Length == 0)
        {
            ModelState.AddModelError(nameof(model.CvFile), "Please attach your CV document (PDF, DOC, DOCX up to 5MB).");
        }
        else
        {
            var cvValidation = CvFileValidator.Validate(model.CvFile);
            if (!cvValidation.IsValid)
            {
                ModelState.AddModelError(nameof(model.CvFile), cvValidation.ErrorMessage ?? "Invalid file format or size.");
            }
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var application = await _recruitmentService.SubmitApplicationAsync(new SubmitJobApplicationDto
            {
                JobPostingId = id,
                FullName = model.FullName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                CoverLetter = model.CoverLetter,
                CvFile = model.CvFile
            });

            var successModel = new ApplySuccessViewModel
            {
                CandidateName = application.FullName,
                JobTitle = posting.Title,
                Email = application.Email,
                SubmittedAt = application.SubmittedAt
            };

            return View("ApplySuccess", successModel);
        }
        catch (BusinessRuleException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred during application submission for Job {JobId}", id);
            ModelState.AddModelError(string.Empty, "An unexpected error occurred while processing your application. Please try again later.");
            return View(model);
        }
    }
}
