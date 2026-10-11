namespace HRSystem.Common.Constants;

public static class RoleNames
{
    public const string CEO = "CEO";
    public const string DepartmentHead = "DepartmentHead";
    public const string Employee = "Employee";

    /// <summary>
    /// Backward-compatibility alias for CEO.
    /// </summary>
    public const string HR = CEO;

    public const string AllRolesCsv = $"{CEO},{DepartmentHead},{Employee}";

    public static readonly string[] AllRoles = [CEO, DepartmentHead, Employee];
}
