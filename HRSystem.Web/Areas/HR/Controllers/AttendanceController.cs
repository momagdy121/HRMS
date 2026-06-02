using HRSystem.Business.Interfaces.Services;
using HRSystem.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Web.Areas.HR.Controllers;

public class AttendanceController : HRBaseController
{
    public AttendanceController(ICurrentUserService currentUser, IUnitOfWork unitOfWork)
        : base(currentUser, unitOfWork)
    {
    }

    public async Task<IActionResult> Index()
    {
        await SetLayoutAsync("Attendance", "Search attendance records...");
        return View();
    }
}
