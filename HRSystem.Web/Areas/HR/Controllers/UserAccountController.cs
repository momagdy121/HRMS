using HRSystem.Business.Interfaces.Services;
using HRSystem.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Web.Areas.HR.Controllers;

public class UserAccountController : HRBaseController
{
    public UserAccountController(ICurrentUserService currentUser, IUnitOfWork unitOfWork)
        : base(currentUser, unitOfWork)
    {
    }

    public async Task<IActionResult> Index()
    {
        await SetLayoutAsync("UserAccounts", "Search user accounts...");
        return View();
    }
}
