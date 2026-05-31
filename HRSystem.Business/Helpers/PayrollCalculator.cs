using HRSystem.Data.Models;

namespace HRSystem.Business.Helpers;

public static class PayrollCalculator
{
    public static void ApplyTotals(Payroll payroll, decimal bonusTotal, decimal deductionTotal)
    {
        payroll.TotalBonus = bonusTotal;
        payroll.TotalDeduction = deductionTotal;
        payroll.NetSalary = payroll.BaseSalary + bonusTotal - deductionTotal;
    }
}
