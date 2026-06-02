using HRSystem.Business.Interfaces.Services;
using HRSystem.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Web.Areas.Employee.Controllers;

public class AttendanceController : EmployeeBaseController
{
    public AttendanceController(ICurrentUserService currentUser, IUnitOfWork unitOfWork)
        : base(currentUser, unitOfWork)
    {
    }

    public async Task<IActionResult> Index()
    {
        await SetLayoutAsync("Attendance", "Search attendance...");
        return View();
    }
}
