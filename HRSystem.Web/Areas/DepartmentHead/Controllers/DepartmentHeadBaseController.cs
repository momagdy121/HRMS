using HRSystem.Business.Interfaces.Services;
using HRSystem.Common.Constants;
using HRSystem.Data.Interfaces;
using HRSystem.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Web.Areas.DepartmentHead.Controllers;

[Area("DepartmentHead")]
[Authorize(Roles = RoleNames.DepartmentHead)]
public abstract class DepartmentHeadBaseController : Controller
{
    protected ICurrentUserService CurrentUser { get; }
    protected IUnitOfWork UnitOfWork { get; }

    protected DepartmentHeadBaseController(ICurrentUserService currentUser, IUnitOfWork unitOfWork)
    {
        CurrentUser = currentUser;
        UnitOfWork = unitOfWork;
    }

    protected Task SetLayoutAsync(string activePage, string? searchPlaceholder = null) =>
        AreaLayoutHelper.SetAsync(this, CurrentUser, UnitOfWork, "DeptHead", activePage, searchPlaceholder);
}
