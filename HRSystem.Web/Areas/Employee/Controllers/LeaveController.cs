using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Web.Areas.Employee.Controllers;

[Area("Employee")]
public class LeaveController : Controller
{
    // Employee: My Leave
    public IActionResult Index()
    {
        ViewBag.Role = "Employee";
        ViewBag.ActivePage = "Leave";
        ViewBag.UserName = "Alice Brown";
        ViewBag.UserTitle = "Employee — Engineering";
        ViewBag.SearchPlaceholder = "Search leave...";
        return View();
    }
}
