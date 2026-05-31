using HRSystem.Data.Models;

namespace HRSystem.Business.Helpers;

public static class DepartmentLifecycle
{
    public static void MarkDeleted(Department department) =>
        department.IsDeleted = true;

    public static void MarkRestored(Department department) =>
        department.IsDeleted = false;
}
