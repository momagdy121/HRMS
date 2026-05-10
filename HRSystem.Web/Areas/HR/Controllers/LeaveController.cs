using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Web.Areas.HR.Controllers;

[Area("HR")]
public class LeaveController : Controller
{
    public IActionResult Index()
    {
        ViewBag.Role = "HR";
        ViewBag.ActivePage = "Leave";
        ViewBag.UserName = "Sarah Connor";
        ViewBag.UserTitle = "HR Director";
        ViewBag.SearchPlaceholder = "Search leave requests...";
        return View();
    }
}
