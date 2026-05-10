using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Web.Areas.Employee.Controllers;

[Area("Employee")]
public class PayrollController : Controller
{
    // Employee: My Payslips
    public IActionResult Index()
    {
        ViewBag.Role = "Employee";
        ViewBag.ActivePage = "Payroll";
        ViewBag.UserName = "Alice Brown";
        ViewBag.UserTitle = "Employee — Engineering";
        ViewBag.SearchPlaceholder = "Search payslips...";
        return View();
    }
}
