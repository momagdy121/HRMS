using HRSystem.Data.Models;

namespace HRSystem.Web.Helpers;

public static class AttendanceDisplayHelper
{
    public static string GetStatusLabel(Attendance attendance)
    {
        if (!attendance.CheckInTime.HasValue)
            return "Absent";

        if (!attendance.CheckOutTime.HasValue)
            return "In progress";

        return IsLate(attendance) ? "Late" : "Present";
    }

    public static string GetStatusBadgeClass(string label) =>
        label switch
        {
            "Present" => "bg-green-50 text-green-700",
            "Late" => "bg-yellow-50 text-amber-700",
            "Absent" => "bg-red-50 text-red-700",
            "In progress" => "bg-slate-100 text-slate-600",
            _ => "bg-slate-100 text-slate-600"
        };

    public static bool IsLate(Attendance attendance)
    {
        if (!attendance.CheckInTime.HasValue)
            return false;

        var t = attendance.CheckInTime.Value.TimeOfDay;
        return t > new TimeSpan(9, 0, 0);
    }

    public static string FormatTime(DateTime? dt) =>
        dt.HasValue ? dt.Value.ToLocalTime().ToString("hh:mm tt") : "—";

    public static string FormatDate(DateOnly date) =>
        date.ToString("MMM d, yyyy");

    public static string FormatDuration(DateTime? checkIn, DateTime? checkOut)
    {
        if (!checkIn.HasValue)
            return "—";

        if (!checkOut.HasValue)
            return "In progress";

        var duration = checkOut.Value - checkIn.Value;
        if (duration.TotalMinutes < 0)
            return "—";

        var hours = (int)duration.TotalHours;
        var mins = duration.Minutes;
        return $"{hours}h {mins}m";
    }
}

