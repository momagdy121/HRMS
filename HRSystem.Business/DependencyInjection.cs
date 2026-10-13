using HRSystem.Business.DTOs;
using HRSystem.Business.Interfaces.Policies;
using HRSystem.Business.Interfaces.Services;
using HRSystem.Business.Policies;
using HRSystem.Business.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HRSystem.Business;

public static class DependencyInjection
{
    public static IServiceCollection AddHrmsBusinessServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        services.Configure<BravoApiSettings>(configuration.GetSection(BravoApiSettings.SectionName));
        services.AddScoped<IEmailService, BrevoEmailService>();
        services.AddSingleton<IBackgroundEmailQueue, BackgroundEmailQueue>();
        services.AddHostedService<EmailBackgroundWorker>();

        services.Configure<CloudinarySettings>(configuration.GetSection(CloudinarySettings.SectionName));
        services.AddScoped<ICloudinaryService, CloudinaryService>();

        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IWorkflowNotificationService, WorkflowNotificationService>();

        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<IPayrollService, PayrollService>();
        services.AddScoped<ILeaveService, LeaveService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<IUserAccountService, UserAccountService>();
        services.AddScoped<IRecruitmentService, RecruitmentService>();

        services.AddScoped<IDepartmentManagerPolicy, DepartmentManagerPolicy>();
        services.AddScoped<IEmployeeDeletionPolicy, EmployeeDeletionPolicy>();
        services.AddScoped<IDepartmentDeletionPolicy, DepartmentDeletionPolicy>();
        services.AddScoped<ITaskAssignmentPolicy, TaskAssignmentPolicy>();

        return services;
    }
}
