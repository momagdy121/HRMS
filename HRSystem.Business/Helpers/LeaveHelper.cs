namespace HRSystem.Business.Helpers;

public static class LeaveHelper
{
    public static int CalendarDays(DateOnly start, DateOnly end) =>
        end.DayNumber - start.DayNumber + 1;
}
