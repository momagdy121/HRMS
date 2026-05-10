using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Web.Areas.Employee.Controllers;

[Area("Employee")]
public class DashboardController : Controller
{
    // Employee Dashboard
    public IActionResult Index()
    {
        ViewBag.Role = "Employee";
        ViewBag.ActivePage = "Dashboard";
        ViewBag.UserName = "Alice Brown";
        ViewBag.UserTitle = "Employee — Engineering";
        ViewBag.SearchPlaceholder = "Search tasks, documents...";
        return View();
    }
}
