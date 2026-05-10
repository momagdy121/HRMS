using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Web.Areas.DepartmentHead.Controllers;

[Area("DepartmentHead")]
public class DashboardController : Controller
{
    // DeptHead Dashboard
    public IActionResult Index()
    {
        ViewBag.Role = "DeptHead";
        ViewBag.ActivePage = "Dashboard";
        ViewBag.UserName = "Bob Johnson";
        ViewBag.UserTitle = "Department Head — Engineering";
        ViewBag.SearchPlaceholder = "Search...";
        return View();
    }

    // DeptHead My Department
    public IActionResult MyDepartment()
    {
        ViewBag.Role = "DeptHead";
        ViewBag.ActivePage = "MyDepartment";
        ViewBag.UserName = "Bob Johnson";
        ViewBag.UserTitle = "Department Head — Engineering";
        ViewBag.SearchPlaceholder = "Search team members...";
        return View();
    }
}
