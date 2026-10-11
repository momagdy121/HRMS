namespace HRSystem.Common.Constants;

public static class RoleHierarchy
{
    public static int GetRoleLevel(string? role) => role switch
    {
        RoleNames.CEO => 3,
        RoleNames.DepartmentHead => 2,
        RoleNames.Employee => 1,
        _ => 0
    };

    public static bool CanAssignRole(string? assignerRole, string targetRole)
    {
        if (string.IsNullOrWhiteSpace(assignerRole))
            return false;

        // Nobody can assign the CEO role via employee creation/hiring:
        if (string.Equals(targetRole, RoleNames.CEO, StringComparison.OrdinalIgnoreCase))
            return false;

        var assignerLevel = GetRoleLevel(assignerRole);
        var targetLevel = GetRoleLevel(targetRole);

        // Assigner role must be strictly higher than the target role (cannot assign equal or higher):
        return assignerLevel > targetLevel && targetLevel > 0;
    }

    public static IEnumerable<string> GetAssignableRoles(string? assignerRole)
    {
        if (string.IsNullOrWhiteSpace(assignerRole))
            return [];

        var assignerLevel = GetRoleLevel(assignerRole);
        return RoleNames.AllRoles
            .Where(r => r != RoleNames.CEO && GetRoleLevel(r) < assignerLevel && GetRoleLevel(r) > 0);
    }
}
