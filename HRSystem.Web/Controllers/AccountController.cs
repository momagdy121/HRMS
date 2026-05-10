using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Web.Controllers;

public class AccountController : Controller
{
    public IActionResult Login()
    {
        ViewBag.HideShell = true;
        ViewBag.Title = "HRMS Portal - Login";
        return View();
    }

    [HttpPost]
    public IActionResult Login(string email, string password)
    {
        ViewBag.HideShell = true;
        ViewBag.Title = "HRMS Portal - Login";
        return View();
    }

    public IActionResult ForgotPassword()
    {
        ViewBag.HideShell = true;
        ViewBag.Title = "Forgot Password - HRMS Portal";
        return View();
    }

    public IActionResult ResetPassword(string? email, string? token)
    {
        ViewBag.HideShell = true;
        ViewBag.Title = "Reset Password - HRMS Portal";
        return View();
    }

    public IActionResult Logout()
    {
        return RedirectToAction("Login");
    }
}
