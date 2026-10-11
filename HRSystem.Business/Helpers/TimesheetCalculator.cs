using HRSystem.Business.DTOs.Attendance;
using HRSystem.Data.Models;

namespace HRSystem.Business.Helpers;

public static class TimesheetCalculator
{
    private static readonly TimeOnly LateThreshold = new(9, 0);

    public static MonthlyTimesheetDto Build(int employeeId, int year, int month, IReadOnlyList<Attendance> records)
    {
        var byDate = records.ToDictionary(r => r.Date);
        var start = new DateOnly(year, month, 1);
        var end = start.AddMonths(1).AddDays(-1);

        var days = new List<MonthlyTimesheetDayDto>();
        var daysPresent = 0;
        var weekdayAbsences = 0;
        var lateCheckIns = 0;
        double totalHours = 0;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        for (var date = start; date <= end; date = date.AddDays(1))
        {
            var isWeekday = date.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday);
            var isFuture = date > today;
            byDate.TryGetValue(date, out var record);

            var isPresent = record?.CheckInTime != null;
            if (isPresent)
                daysPresent++;

            var isLate = false;
            double? hours = null;
            if (record?.CheckInTime != null)
            {
                var checkInLocal = TimeOnly.FromDateTime(record.CheckInTime.Value);
                isLate = checkInLocal > LateThreshold;
                if (isLate)
                    lateCheckIns++;

                if (record.CheckOutTime.HasValue)
                {
                    hours = (record.CheckOutTime.Value - record.CheckInTime.Value).TotalHours;
                    if (hours < 0)
                        hours = 0;
                    totalHours += hours.Value;
                }
            }
            else if (isWeekday && !isFuture)
            {
                weekdayAbsences++;
            }

            days.Add(new MonthlyTimesheetDayDto
            {
                Date = date,
                CheckInTime = record?.CheckInTime,
                CheckOutTime = record?.CheckOutTime,
                IsPresent = isPresent,
                IsLate = isLate,
                IsWeekday = isWeekday,
                IsFuture = isFuture,
                HoursWorked = hours
            });
        }

        return new MonthlyTimesheetDto
        {
            EmployeeId = employeeId,
            Year = year,
            Month = month,
            DaysPresent = daysPresent,
            WeekdayAbsences = weekdayAbsences,
            LateCheckIns = lateCheckIns,
            TotalHours = Math.Round(totalHours, 2),
            Days = days
        };
    }
}
