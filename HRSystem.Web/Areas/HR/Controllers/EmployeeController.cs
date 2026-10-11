using HRSystem.Business.DTOs.Employees;
using HRSystem.Business.Exceptions;
using HRSystem.Business.Helpers;
using HRSystem.Business.Interfaces.Services;
using HRSystem.Common.Constants;
using HRSystem.Data.Interfaces;
using HRSystem.Data.Models;
using HRSystem.Web.Helpers;
using HRSystem.Web.ViewModels.HR;
using Microsoft.AspNetCore.Mvc;
using EmployeeEntity = HRSystem.Data.Models.Employee;

namespace HRSystem.Web.Areas.HR.Controllers;

public class EmployeeController : HRBaseController
{
    private const int PageSize = PaginationDefaults.DefaultPageSize;
    private readonly IEmployeeService _employeeService;
    private readonly IAttendanceService _attendanceService;

    public EmployeeController(
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork,
        IEmployeeService employeeService,
        IAttendanceService attendanceService)
        : base(currentUser, unitOfWork)
    {
        _employeeService = employeeService;
        _attendanceService = attendanceService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, int? departmentId = null, string? search = null, int? editId = null, bool showCreate = false)
    {
        await SetLayoutAsync("Employees", "Search employees...");
        return View(await BuildIndexModelAsync(page, departmentId, search, editId, showCreate));
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        await SetLayoutAsync("Employees", "Search employees...");

        var employee = await UnitOfWork.Employees.GetByIdAsync(id);
        if (employee == null)
            return NotFound();

        var departments = await HrDisplayHelper.LoadDepartmentsAsync(UnitOfWork);
        departments.TryGetValue(employee.DepartmentId, out var dept);

        EmployeeEntity? manager = null;
        if (dept?.ManagerId > 0)
        {
            manager = await UnitOfWork.Employees.GetByIdAsync(dept.ManagerId);
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var totalDays = Math.Max(0, today.DayNumber - employee.HireDate.DayNumber);
        var years = totalDays / 365;
        var months = (totalDays % 365) / 30;
        string tenure;
        if (totalDays < 30)
            tenure = $"{totalDays} day{(totalDays != 1 ? "s" : "")}";
        else if (years == 0)
            tenure = $"{months} month{(months != 1 ? "s" : "")}";
        else
            tenure = $"{years} yr{(years != 1 ? "s" : "")} {months} mo{(months != 1 ? "s" : "")}";

        var balances = await UnitOfWork.LeaveBalances.GetByEmployeeAndYearAsync(id, DateTime.UtcNow.Year);
        var balanceItems = balances.Select(b => new EmployeeLeaveBalanceItemViewModel
        {
            LeaveType = b.LeaveType,
            TotalDays = b.TotalDays,
            UsedDays = b.UsedDays
        }).ToList();

        var leavesPaged = await UnitOfWork.LeaveRequests.GetByEmployeePagedAsync(id, 1, 5);
        var recentLeaves = leavesPaged.Items.Select(l => new EmployeeRecentLeaveViewModel
        {
            Id = l.Id,
            LeaveType = l.LeaveType,
            StartDate = l.StartDate,
            EndDate = l.EndDate,
            DaysCount = Math.Max(1, l.EndDate.DayNumber - l.StartDate.DayNumber + 1),
            Status = l.Status,
            Reason = l.Reason
        }).ToList();

        var payrollsPaged = await UnitOfWork.Payrolls.GetByEmployeePagedAsync(id, 1, 5);
        var recentPayrolls = payrollsPaged.Items.Select(p => new EmployeeRecentPayrollViewModel
        {
            Id = p.Id,
            Month = p.Month,
            Year = p.Year,
            Period = new DateTime(p.Year, p.Month, 1).ToString("MMM yyyy"),
            BaseSalary = p.BaseSalary,
            TotalBonus = p.TotalBonus,
            TotalDeduction = p.TotalDeduction,
            NetSalary = p.NetSalary,
            Status = p.Status
        }).ToList();

        var attendancesPaged = await UnitOfWork.Attendances.GetByEmployeePagedAsync(id, 1, 7);
        var recentAttendances = attendancesPaged.Items.Select(a =>
        {
            string duration = "—";
            if (a.CheckInTime.HasValue && a.CheckOutTime.HasValue)
            {
                var dur = a.CheckOutTime.Value - a.CheckInTime.Value;
                duration = $"{dur.TotalHours:0.1} hrs";
            }

            string status = "Absent";
            if (a.CheckInTime.HasValue)
            {
                status = a.CheckInTime.Value.TimeOfDay > new TimeSpan(9, 15, 0) ? "Late Check-in" : "Present";
            }

            return new EmployeeRecentAttendanceViewModel
            {
                Date = a.Date,
                CheckInTime = a.CheckInTime,
                CheckOutTime = a.CheckOutTime,
                DurationLabel = duration,
                StatusLabel = status
            };
        }).ToList();

        var timesheetDto = await _attendanceService.GetMonthlyTimesheetAsync(id, DateTime.UtcNow.Year, DateTime.UtcNow.Month);

        var tasksPaged = await UnitOfWork.EmployeeTasks.GetByAssignedToPagedAsync(id, 1, 5);
        var recentTasks = tasksPaged.Items.Select(t => new EmployeeRecentTaskViewModel
        {
            Id = t.Id,
            Title = t.Title,
            Status = t.Status.ToString(),
            DueDate = t.DueDate,
            IsCompleted = t.Status == global::HRSystem.Common.Enums.TaskStatus.Completed
        }).ToList();

        var activeTasksCount = tasksPaged.Items.Count(t => t.Status != global::HRSystem.Common.Enums.TaskStatus.Completed);

        var isManager = dept?.ManagerId == employee.Id;

        var model = new EmployeeDetailViewModel
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Initials = HrDisplayHelper.GetInitials(employee.FirstName, employee.LastName),
            AvatarUrl = employee.ProfilePictureUrl,
            Email = employee.Email ?? string.Empty,
            PhoneNumber = employee.PhoneNumber,
            DepartmentId = employee.DepartmentId,
            DepartmentName = dept?.Name ?? "—",
            IsManager = isManager,
            IsHR = employee.IsHR,
            IsActive = employee.IsActive,
            IsDeleted = employee.IsDeleted,
            HireDate = employee.HireDate,
            Salary = employee.Salary,
            ManagerName = manager != null ? $"{manager.FirstName} {manager.LastName}" : null,
            ManagerId = manager?.Id,
            Tenure = tenure,
            LeaveBalances = balanceItems,
            RecentLeaves = recentLeaves,
            RecentPayrolls = recentPayrolls,
            RecentAttendances = recentAttendances,
            DaysPresentThisMonth = timesheetDto.DaysPresent,
            AbsencesThisMonth = timesheetDto.WeekdayAbsences,
            AssignedTasks = recentTasks,
            ActiveTasksCount = activeTasksCount,
            EditForm = new EditEmployeeViewModel
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Email = employee.Email ?? string.Empty,
                DepartmentId = employee.DepartmentId,
                Salary = employee.Salary,
                HireDate = employee.HireDate,
                IsActive = employee.IsActive
            }
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Deleted(int page = 1, string? search = null)
    {
        await SetLayoutAsync("Employees", "Search deleted employees...");

        var departments = await HrDisplayHelper.LoadDepartmentsAsync(UnitOfWork);
        var paged = await _employeeService.GetDeletedAsync(page, PageSize, search);

        return View("Index", new EmployeeIndexViewModel
        {
            Employees = paged.Items.Select(e => MapListItem(e, departments, new Dictionary<int, bool>())).ToList(),
            Departments = ToDepartmentOptions(departments),
            Page = paged.Page,
            TotalPages = paged.TotalPages,
            TotalCount = paged.TotalCount,
            PageSize = paged.PageSize
        });
    }

    [HttpGet]
    public async Task<IActionResult> ExportCsv(int? departmentId = null, string? search = null)
    {
        var employees = await UnitOfWork.Employees.GetActiveForPayrollAsync(departmentId);
        var departmentsMap = await HrDisplayHelper.LoadDepartmentsAsync(UnitOfWork);

        var headers = new[]
        {
            "Employee ID",
            "First Name",
            "Last Name",
            "Email",
            "Department",
            "Role",
            "Base Salary",
            "Hire Date",
            "Status"
        };

        var rows = new List<IEnumerable<object?>>();
        foreach (var emp in employees)
        {
            var fullName = $"{emp.FirstName} {emp.LastName}";
            if (!string.IsNullOrWhiteSpace(search) &&
                !fullName.Contains(search, StringComparison.OrdinalIgnoreCase) &&
                !(emp.Email?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false))
            {
                continue;
            }

            departmentsMap.TryGetValue(emp.DepartmentId, out var dept);

            rows.Add(new object?[]
            {
                emp.Id,
                emp.FirstName,
                emp.LastName,
                emp.Email ?? string.Empty,
                dept?.Name ?? "—",
                emp.IsHR ? "CEO" : (dept?.ManagerId == emp.Id ? "Department Head" : "Employee"),
                emp.Salary,
                emp.HireDate.ToString("yyyy-MM-dd"),
                emp.IsActive ? "Active" : "Inactive"
            });
        }

        var csvBytes = CsvExportHelper.CreateCsvBytes(headers, rows);
        var fileName = $"Employees_Directory_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv";
        return File(csvBytes, "text/csv; charset=utf-8", fileName);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind(Prefix = "CreateForm")] CreateEmployeeViewModel model)
    {
        var assignerRole = CurrentUser.IsCEO() ? RoleNames.CEO : (CurrentUser.IsDepartmentHead() ? RoleNames.DepartmentHead : RoleNames.Employee);
        if (!RoleHierarchy.CanAssignRole(assignerRole, model.Role))
        {
            ModelState.AddModelError("CreateForm.Role", $"You cannot assign a role equal to or higher than your position ({assignerRole}), nor can you create another CEO.");
        }

        if (!ModelState.IsValid)
        {
            await SetLayoutAsync("Employees", "Search employees...");
            var invalid = await BuildIndexModelAsync(1, null, showCreate: true, createForm: model);
            return View("Index", invalid);
        }

        try
        {
            await _employeeService.CreateAsync(new CreateEmployeeDto
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                DepartmentId = model.DepartmentId,
                Salary = model.Salary,
                HireDate = model.HireDate,
                Role = model.Role,
                InitialPassword = model.InitialPassword
            });

            TempData["Success"] = "Employee created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleException ex)
        {
            await SetLayoutAsync("Employees", "Search employees...");
            var invalid = await BuildIndexModelAsync(1, null, showCreate: true, createForm: model);
            var fieldPrefix = ex.Message.Contains("email", StringComparison.OrdinalIgnoreCase)
                ? "CreateForm.Email"
                : "CreateForm.InitialPassword";
            ModalValidationHelper.AddFormErrors(ModelState, fieldPrefix, ex.Message);
            return View("Index", invalid);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([Bind(Prefix = "EditForm")] EditEmployeeViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await SetLayoutAsync("Employees", "Search employees...");
            var invalid = await BuildIndexModelAsync(1, null, editId: model.Id);
            invalid.EditForm = model;
            return View("Index", invalid);
        }

        try
        {
            await _employeeService.UpdateAsync(new UpdateEmployeeDto
            {
                Id = model.Id,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                DepartmentId = model.DepartmentId,
                Salary = model.Salary,
                HireDate = model.HireDate,
                IsActive = model.IsActive
            });

            TempData["Success"] = "Employee updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleException ex)
        {
            await SetLayoutAsync("Employees", "Search employees...");
            var invalid = await BuildIndexModelAsync(1, null, editId: model.Id);
            invalid.EditForm = model;
            ModalValidationHelper.AddFormErrors(ModelState, "EditForm.Email", ex.Message);
            return View("Index", invalid);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _employeeService.SoftDeleteAsync(id);
        TempData["Success"] = "Employee deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id)
    {
        await _employeeService.RestoreAsync(id);
        TempData["Success"] = "Employee restored successfully.";
        return RedirectToAction(nameof(Deleted));
    }

    private async Task<EmployeeIndexViewModel> BuildIndexModelAsync(
        int page,
        int? departmentId,
        string? search = null,
        int? editId = null,
        bool showCreate = false,
        CreateEmployeeViewModel? createForm = null)
    {
        var departments = await HrDisplayHelper.LoadDepartmentsAsync(UnitOfWork);
        var paged = departmentId.HasValue
            ? await _employeeService.GetByDepartmentAsync(departmentId.Value, page, PageSize, search)
            : await _employeeService.GetAllAsync(page, PageSize, search);

        var activeManagerIds = departments.Values.Select(d => d.ManagerId).ToHashSet();
        var canDeleteMap = paged.Items.ToDictionary(e => e.Id, e => !activeManagerIds.Contains(e.Id));

        var model = new EmployeeIndexViewModel
        {
            Employees = paged.Items.Select(e => MapListItem(e, departments, canDeleteMap)).ToList(),
            Departments = ToDepartmentOptions(departments),
            CreateForm = createForm ?? new CreateEmployeeViewModel(),
            Page = paged.Page,
            TotalPages = paged.TotalPages,
            TotalCount = paged.TotalCount,
            PageSize = paged.PageSize,
            DepartmentFilter = departmentId,
            ShowCreateModal = showCreate
        };

        if (editId.HasValue)
        {
            var employee = await _employeeService.GetByIdAsync(editId.Value);
            model.EditForm = new EditEmployeeViewModel
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Email = employee.Email ?? string.Empty,
                DepartmentId = employee.DepartmentId,
                Salary = employee.Salary,
                HireDate = employee.HireDate,
                IsActive = employee.IsActive
            };
        }

        return model;
    }

    private static IReadOnlyList<DepartmentOptionViewModel> ToDepartmentOptions(IReadOnlyDictionary<int, Department> departments) =>
        departments.Values.Select(d => new DepartmentOptionViewModel { Id = d.Id, Name = d.Name }).OrderBy(d => d.Name).ToList();

    private static EmployeeListItemViewModel MapListItem(
        Data.Models.Employee employee,
        IReadOnlyDictionary<int, Data.Models.Department> departments,
        IReadOnlyDictionary<int, bool> canDeleteMap)
    {
        departments.TryGetValue(employee.DepartmentId, out var department);
        var isManager = department?.ManagerId == employee.Id;

        return new EmployeeListItemViewModel
        {
            Id = employee.Id,
            FullName = $"{employee.FirstName} {employee.LastName}",
            Email = employee.Email ?? string.Empty,
            DepartmentId = employee.DepartmentId,
            DepartmentName = department?.Name ?? "—",
            IsManager = isManager,
            IsCEO = employee.IsHR,
            HireDate = employee.HireDate,
            CanDelete = canDeleteMap.GetValueOrDefault(employee.Id, !isManager),
            Initials = HrDisplayHelper.GetInitials(employee.FirstName, employee.LastName)
        };
    }
}
