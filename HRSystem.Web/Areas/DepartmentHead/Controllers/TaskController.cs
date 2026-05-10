using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Web.Areas.DepartmentHead.Controllers;

[Area("DepartmentHead")]
public class TaskController : Controller
{
    // DeptHead: Manage Tasks
    public IActionResult Index()
    {
        ViewBag.Role = "DeptHead";
        ViewBag.ActivePage = "Tasks";
        ViewBag.UserName = "Bob Johnson";
        ViewBag.UserTitle = "Department Head — Engineering";
        ViewBag.SearchPlaceholder = "Search tasks...";
        return View();
    }
}
