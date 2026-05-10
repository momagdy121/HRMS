using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Web.Areas.HR.Controllers;

[Area("HR")]
public class AttendanceController : Controller
{
    public IActionResult Index()
    {
        ViewBag.Role = "HR";
        ViewBag.ActivePage = "Attendance";
        ViewBag.UserName = "Sarah Connor";
        ViewBag.UserTitle = "HR Director";
        ViewBag.SearchPlaceholder = "Search attendance...";
        return View();
    }
}
