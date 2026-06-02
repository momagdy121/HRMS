using HRSystem.Business.Interfaces.Services;
using HRSystem.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Web.Areas.DepartmentHead.Controllers;

public class AttendanceController : DepartmentHeadBaseController
{
    public AttendanceController(ICurrentUserService currentUser, IUnitOfWork unitOfWork)
        : base(currentUser, unitOfWork)
    {
    }

    public async Task<IActionResult> Index()
    {
        await SetLayoutAsync("Attendance", "Search team attendance...");
        return View();
    }
}
