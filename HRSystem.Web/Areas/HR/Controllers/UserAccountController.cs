using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Web.Areas.HR.Controllers;

[Area("HR")]
public class UserAccountController : Controller
{
    public IActionResult Index()
    {
        ViewBag.Role = "HR";
        ViewBag.ActivePage = "UserAccounts";
        ViewBag.UserName = "Sarah Connor";
        ViewBag.UserTitle = "HR Director";
        ViewBag.SearchPlaceholder = "Search user accounts...";
        return View();
    }
}
