using HRSystem.Business.Interfaces.Policies;
using HRSystem.Business.Interfaces.Services;
using HRSystem.Business.Policies;
using HRSystem.Business.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HRSystem.Business;

public static class DependencyInjection
{
    public static IServiceCollection AddHrmsBusinessServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<IPayrollService, PayrollService>();
        services.AddScoped<ILeaveService, LeaveService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<IUserAccountService, UserAccountService>();

        services.AddScoped<IDepartmentManagerPolicy, DepartmentManagerPolicy>();
        services.AddScoped<IEmployeeDeletionPolicy, EmployeeDeletionPolicy>();
        services.AddScoped<IDepartmentDeletionPolicy, DepartmentDeletionPolicy>();
        services.AddScoped<ITaskAssignmentPolicy, TaskAssignmentPolicy>();

        return services;
    }
}
