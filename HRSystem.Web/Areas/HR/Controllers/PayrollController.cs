using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Web.Areas.HR.Controllers;

[Area("HR")]
public class PayrollController : Controller
{
    public IActionResult Index()
    {
        ViewBag.Role = "HR";
        ViewBag.ActivePage = "Payroll";
        ViewBag.UserName = "Sarah Connor";
        ViewBag.UserTitle = "HR Director";
        ViewBag.SearchPlaceholder = "Search payroll records...";
        return View();
    }

    public IActionResult ProcessStep1()
    {
        ViewBag.Role = "HR";
        ViewBag.ActivePage = "Payroll";
        ViewBag.UserName = "Sarah Connor";
        ViewBag.UserTitle = "HR Director";
        return View();
    }

    public IActionResult ProcessStep2()
    {
        ViewBag.Role = "HR";
        ViewBag.ActivePage = "Payroll";
        ViewBag.UserName = "Sarah Connor";
        ViewBag.UserTitle = "HR Director";
        return View();
    }

    public IActionResult ProcessStep3(bool showModal = false)
    {
        ViewBag.Role = "HR";
        ViewBag.ActivePage = "Payroll";
        ViewBag.UserName = "Sarah Connor";
        ViewBag.UserTitle = "HR Director";
        
        if (showModal)
        {
            return View("ProcessStep3Modal");
        }
        return View();
    }

    public IActionResult ProcessStep4()
    {
        ViewBag.Role = "HR";
        ViewBag.ActivePage = "Payroll";
        ViewBag.UserName = "Sarah Connor";
        ViewBag.UserTitle = "HR Director";
        return View();
    }

    public IActionResult MarkAsPaidSuccess()
    {
        ViewBag.Role = "HR";
        ViewBag.ActivePage = "Payroll";
        ViewBag.UserName = "Sarah Connor";
        ViewBag.UserTitle = "HR Director";
        return View();
    }

    public IActionResult PayslipDetail()
    {
        ViewBag.Role = "HR";
        ViewBag.ActivePage = "Payroll";
        ViewBag.UserName = "Sarah Connor";
        ViewBag.UserTitle = "HR Director";
        return View();
    }
}
