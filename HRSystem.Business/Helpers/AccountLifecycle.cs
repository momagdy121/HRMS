using HRSystem.Data.Models;

namespace HRSystem.Business.Helpers;

public static class AccountLifecycle
{
    public static void MarkPasswordChanged(ApplicationUser user) =>
        user.IsPasswordChangeRequired = false;
}
