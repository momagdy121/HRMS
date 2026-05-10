using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Web.Areas.DepartmentHead.Controllers;

[Area("DepartmentHead")]
public class LeaveController : Controller
{
    // DeptHead: Team Leave
    public IActionResult Index()
    {
        ViewBag.Role = "DeptHead";
        ViewBag.ActivePage = "Leave";
        ViewBag.UserName = "Bob Johnson";
        ViewBag.UserTitle = "Department Head — Engineering";
        ViewBag.SearchPlaceholder = "Search leave requests...";
        return View();
    }
}
