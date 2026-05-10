using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Web.Areas.Employee.Controllers;

[Area("Employee")]
public class TaskController : Controller
{
    // Employee: My Tasks
    public IActionResult Index()
    {
        ViewBag.Role = "Employee";
        ViewBag.ActivePage = "Tasks";
        ViewBag.UserName = "Alice Brown";
        ViewBag.UserTitle = "Employee — Engineering";
        ViewBag.SearchPlaceholder = "Search tasks...";
        return View();
    }
}
