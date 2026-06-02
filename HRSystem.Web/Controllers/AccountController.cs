using HRSystem.Business.Interfaces.Services;
using HRSystem.Data.Models;
using HRSystem.Web.Helpers;
using HRSystem.Web.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Web.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAccountService _accountService;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IAccountService accountService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _accountService = accountService;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return AuthRedirectHelper.ToRoleDashboard(this);

        ViewBag.HideShell = true;
        ViewBag.Title = "HRMS Portal - Login";
        ViewBag.ReturnUrl = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewBag.HideShell = true;
        ViewBag.Title = "HRMS Portal - Login";
        ViewBag.ReturnUrl = returnUrl;

        if (!ModelState.IsValid)
            return View(model);

        var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return AuthRedirectHelper.ToRoleDashboard(this);
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Account locked. Try again later.");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password. Please try again.");
        }

        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return RedirectToAction(nameof(ComingSoon));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(string? email, string? token)
    {
        return RedirectToAction(nameof(ComingSoon));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ComingSoon()
    {
        ViewBag.HideShell = true;
        ViewBag.Title = "Coming Soon - HRMS Portal";
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        ViewBag.HideShell = true;
        ViewBag.Title = "Access Denied - HRMS Portal";
        return View();
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> ChangePassword()
    {
        ViewBag.HideShell = true;
        ViewBag.Title = "Change Password - HRMS Portal";

        var user = await _userManager.GetUserAsync(User);
        ViewBag.IsRequired = user?.IsPasswordChangeRequired ?? false;

        return View(new ChangePasswordViewModel());
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        ViewBag.HideShell = true;
        ViewBag.Title = "Change Password - HRMS Portal";

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return RedirectToAction(nameof(Login));

        ViewBag.IsRequired = user.IsPasswordChangeRequired;

        if (!ModelState.IsValid)
            return View(model);

        await _accountService.ChangePasswordAsync(user.Id, model.NewPassword);
        await _signInManager.RefreshSignInAsync(user);

        TempData["Success"] = "Your password has been updated.";
        return AuthRedirectHelper.ToRoleDashboard(this);
    }
}
