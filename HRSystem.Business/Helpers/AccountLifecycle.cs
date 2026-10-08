using HRSystem.Data.Models;

namespace HRSystem.Business.Helpers;

public static class AccountLifecycle
{
    public static void MarkPasswordChanged(Employee employee) =>
        employee.IsPasswordChangeRequired = false;
}
