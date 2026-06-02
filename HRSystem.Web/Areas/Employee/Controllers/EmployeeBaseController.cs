using HRSystem.Business.Interfaces.Services;
using HRSystem.Common.Constants;
using HRSystem.Data.Interfaces;
using HRSystem.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Web.Areas.Employee.Controllers;

[Area("Employee")]
[Authorize(Roles = RoleNames.AllRolesCsv)]
public abstract class EmployeeBaseController : Controller
{
    protected ICurrentUserService CurrentUser { get; }
    protected IUnitOfWork UnitOfWork { get; }

    protected EmployeeBaseController(ICurrentUserService currentUser, IUnitOfWork unitOfWork)
    {
        CurrentUser = currentUser;
        UnitOfWork = unitOfWork;
    }

    protected Task SetLayoutAsync(string activePage, string? searchPlaceholder = null) =>
        AreaLayoutHelper.SetAsync(this, CurrentUser, UnitOfWork, "Employee", activePage, searchPlaceholder);
}
