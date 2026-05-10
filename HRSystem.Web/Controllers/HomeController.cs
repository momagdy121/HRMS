using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("Login", "Account");
    }
}
