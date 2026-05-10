using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Web.Areas.HR.Controllers;

[Area("HR")]
public class EmployeeController : Controller
{
    public IActionResult Index()
    {
        ViewBag.Role = "HR";
        ViewBag.ActivePage = "Employees";
        ViewBag.UserName = "Sarah Connor";
        ViewBag.UserTitle = "HR Director";
        ViewBag.SearchPlaceholder = "Search employees...";
        return View();
    }
}
