using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Web.Areas.Employee.Controllers;

[Area("Employee")]
public class AttendanceController : Controller
{
    // Employee: My Attendance
    public IActionResult Index()
    {
        ViewBag.Role = "Employee";
        ViewBag.ActivePage = "Attendance";
        ViewBag.UserName = "Alice Brown";
        ViewBag.UserTitle = "Employee — Engineering";
        ViewBag.SearchPlaceholder = "Search attendance...";
        return View();
    }
}
