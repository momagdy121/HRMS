using HRSystem.Common.Enums;

namespace HRSystem.Web.Helpers;

public static class PayrollDisplayHelper
{
    public static string FormatCurrency(decimal amount) => amount.ToString("C2");

    public static string GetPeriodLabel(int month, int year) =>
        $"{GetMonthName(month)} {year}";

    public static string GetMonthName(int month) =>
        month switch
        {
            1 => "January",
            2 => "February",
            3 => "March",
            4 => "April",
            5 => "May",
            6 => "June",
            7 => "July",
            8 => "August",
            9 => "September",
            10 => "October",
            11 => "November",
            12 => "December",
            _ => month.ToString()
        };

    public static string GetStatusLabel(PayrollStatus status) =>
        status switch
        {
            PayrollStatus.Draft => "Draft",
            PayrollStatus.Approved => "Approved",
            PayrollStatus.Paid => "Paid",
            _ => status.ToString()
        };

    public static string GetStatusBadgeClass(PayrollStatus status) =>
        status switch
        {
            PayrollStatus.Draft => "bg-slate-100 text-slate-600",
            PayrollStatus.Approved => "bg-amber-50 text-amber-600",
            PayrollStatus.Paid => "bg-green-50 text-green-600",
            _ => "bg-slate-100 text-slate-600"
        };

    public static string GetItemTypeLabel(ItemType itemType) =>
        itemType switch
        {
            ItemType.Bonus => "Bonus",
            ItemType.Deduction => "Deduction",
            _ => itemType.ToString()
        };
}
