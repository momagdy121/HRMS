using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Web.Areas.DepartmentHead.Controllers;

[Area("DepartmentHead")]
public class PayrollController : Controller
{
    // DeptHead: Department Payroll
    public IActionResult Index()
    {
        ViewBag.Role = "DeptHead";
        ViewBag.ActivePage = "Payroll";
        ViewBag.UserName = "Bob Johnson";
        ViewBag.UserTitle = "Department Head — Engineering";
        ViewBag.SearchPlaceholder = "Search payroll...";
        return View();
    }
}
