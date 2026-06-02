using HRSystem.Business.Helpers;
using HRSystem.Common.Enums;

namespace HRSystem.Web.Helpers;

public static class LeaveDisplayHelper
{
    public static string GetStatusLabel(LeaveRequestStatus status) =>
        status switch
        {
            LeaveRequestStatus.Pending => "Pending",
            LeaveRequestStatus.Approved => "Approved",
            LeaveRequestStatus.Rejected => "Rejected",
            _ => status.ToString()
        };

    public static string GetStatusBadgeClass(LeaveRequestStatus status) =>
        status switch
        {
            LeaveRequestStatus.Pending => "bg-yellow-50 text-amber-700",
            LeaveRequestStatus.Approved => "bg-green-50 text-green-700",
            LeaveRequestStatus.Rejected => "bg-red-50 text-red-700",
            _ => "bg-slate-100 text-slate-600"
        };

    public static string GetLeaveTypeLabel(LeaveType leaveType) =>
        leaveType switch
        {
            LeaveType.Annual => "Annual",
            LeaveType.Sick => "Sick",
            LeaveType.Unpaid => "Unpaid",
            _ => leaveType.ToString()
        };

    public static string GetLeaveTypeBadgeClass(LeaveType leaveType) =>
        leaveType switch
        {
            LeaveType.Annual => "bg-blue-50 text-blue-700",
            LeaveType.Sick => "bg-orange-50 text-orange-700",
            LeaveType.Unpaid => "bg-slate-100 text-slate-600",
            _ => "bg-slate-100 text-slate-600"
        };

    public static string FormatDateRange(DateOnly start, DateOnly end) =>
        start == end
            ? start.ToString("MMM d, yyyy")
            : $"{start:MMM d} – {end:MMM d, yyyy}";

    public static int GetDayCount(DateOnly start, DateOnly end) =>
        LeaveHelper.CalendarDays(start, end);
}
