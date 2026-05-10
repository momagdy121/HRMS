using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Web.Areas.DepartmentHead.Controllers;

[Area("DepartmentHead")]
public class AttendanceController : Controller
{
    // DeptHead: Team Attendance
    public IActionResult Index()
    {
        ViewBag.Role = "DeptHead";
        ViewBag.ActivePage = "Attendance";
        ViewBag.UserName = "Bob Johnson";
        ViewBag.UserTitle = "Department Head — Engineering";
        ViewBag.SearchPlaceholder = "Search attendance...";
        return View();
    }
}
