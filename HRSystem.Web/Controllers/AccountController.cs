using HRSystem.Business.Exceptions;
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
    private readonly SignInManager<Employee> _signInManager;
    private readonly UserManager<Employee> _userManager;
    private readonly IAccountService _accountService;

    public AccountController(
        SignInManager<Employee> signInManager,
        UserManager<Employee> userManager,
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
        ViewBag.HideShell = true;
        ViewBag.Title = "Forgot Password - HRMS Portal";
        return View(new ForgotPasswordViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        ViewBag.HideShell = true;
        ViewBag.Title = "Forgot Password - HRMS Portal";

        if (!ModelState.IsValid)
            return View(model);

        // Build a callback URL template with placeholders for email and token.
        // The service will replace __EMAIL__ and __TOKEN__ with actual values.
        var resetCallbackUrl = Url.Action(
            nameof(ResetPassword),
            "Account",
            new { email = "__EMAIL__", token = "__TOKEN__" },
            Request.Scheme)!;

        await _accountService.ForgotPasswordAsync(model.Email, resetCallbackUrl);

        // Always show success to prevent email enumeration attacks
        ViewBag.EmailSent = true;
        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(string? email, string? token)
    {
        ViewBag.HideShell = true;
        ViewBag.Title = "Reset Password - HRMS Portal";

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
        {
            ViewBag.InvalidToken = true;
            return View(new ResetPasswordViewModel());
        }

        return View(new ResetPasswordViewModel
        {
            Email = email,
            Token = token
        });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        ViewBag.HideShell = true;
        ViewBag.Title = "Reset Password - HRMS Portal";

        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _accountService.ResetPasswordAsync(model.Email, model.Token, model.NewPassword);
            TempData["Success"] = "Password reset successfully. You can now sign in with your new password.";
            return RedirectToAction(nameof(Login));
        }
        catch (BusinessRuleException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
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
