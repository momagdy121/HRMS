using HRSystem.Business.Interfaces.Services;
using HRSystem.Common.Constants;
using HRSystem.Data.Interfaces;
using HRSystem.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Web.Areas.HR.Controllers;

[Area("HR")]
[Authorize(Roles = RoleNames.HR)]
public abstract class HRBaseController : Controller
{
    protected ICurrentUserService CurrentUser { get; }
    protected IUnitOfWork UnitOfWork { get; }

    protected HRBaseController(ICurrentUserService currentUser, IUnitOfWork unitOfWork)
    {
        CurrentUser = currentUser;
        UnitOfWork = unitOfWork;
    }

    protected Task SetLayoutAsync(string activePage, string? searchPlaceholder = null) =>
        AreaLayoutHelper.SetAsync(this, CurrentUser, UnitOfWork, "HR", activePage, searchPlaceholder);
}
