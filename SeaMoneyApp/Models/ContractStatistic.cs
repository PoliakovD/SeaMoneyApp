using System;
using System.Collections.Generic;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Avalonia;
using SeaMoneyApp.DataAccess;
using SeaMoneyApp.DataAccess.Models;
using Splat;

namespace SeaMoneyApp.Models;

public class ContractStatistic
{
    private ContractStates State { get; set; }
    public Contract? Contract { get; set; }
    public double DurationDays { get; set; }
    public double PassedDays { get; set; }


    public double LeftDays => DurationDays - PassedDays;
    public Dictionary<int, ContractStatisticValue> MonthlyStatistic { get; set; } = [];
    public int ToursInRank { get; set; } = 0;
    public decimal CurrentAchivedWageRub { get; set; }
    public decimal CurrentAchivedWageDollar { get; set; }

    public decimal AverageCource => CurrentAchivedWageDollar == 0?0: CurrentAchivedWageRub / CurrentAchivedWageDollar;
    public decimal DiferenceRub => MaxAvailableWageRub - CurrentAchivedWageRub;
    public decimal DiferenceDollar => MaxAvailableWageDollar - CurrentAchivedWageDollar;
    public decimal MaxAvailableWageRub { get; set; }
    public decimal MaxAvailableWageDollar { get; set; }

    public ContractStatistic(Contract? contract, IEnumerable<WageLog> wageLogs)
    {
        Contract = contract;

        DurationDays = (Contract!.EndDate!.Value - Contract.BeginDate).TotalDays;

        PassedDays = (DateTime.Today.Date - Contract.BeginDate + TimeSpan.FromDays(1)).TotalDays;

        if (PassedDays > DurationDays) PassedDays = DurationDays;
        InitState();
        ToursInRank = Locator.Current.GetService<AppSession>().CurrentAccount.ToursInRank;
        if (wageLogs.Any())
        {
            ToursInRank = wageLogs.FirstOrDefault().ToursInRank;
            
            InitCurrentAchivedWage(wageLogs);
        }
        InitMaxAvailableWage();
    }

    private void InitCurrentAchivedWage(IEnumerable<WageLog> wageLogs)
    {
        var contractWages = wageLogs.Where(x => x.Contract!.Id == Contract!.Id);
        if (contractWages is null) return;
        foreach (var wageLog in contractWages)
        {
            var received = wageLog.AmountInRub;

            var month = wageLog.Date!.Value.Month;
            if (wageLog.Date.Value.Day < 15) --month;

            if (month == 0) month = 12;

            var stat = MonthlyStatistic.ContainsKey(month) ? MonthlyStatistic[month] : new ContractStatisticValue();
            stat.AchivedDollar += Math.Round(wageLog.AmountInDollars!.Value, 2);
            stat.AchivedRub += received!.Value;

            MonthlyStatistic.TryAdd(month, stat);

            CurrentAchivedWageRub += received.Value;
            CurrentAchivedWageDollar += wageLog.AmountInDollars!.Value;
        }
    }

    private void InitMaxAvailableWage()
    {
        int defaultDay = 15;
        var db = Locator.Current.GetService<DataBaseContext>();
        int year = Contract!.BeginDate.Year;
        for (int month = Contract.BeginDate.Month; month != Contract.EndDate!.Value.Month + 1; month++)
        {
            int workedDaysInMonth = 0;
            var daysInMonth = DateTime.DaysInMonth(year, month);

            if (month == Contract.BeginDate.Month)
            {
                workedDaysInMonth = daysInMonth - Contract.BeginDate.Day;
            }
            else if (month == Contract.EndDate.Value.Month)
            {
                workedDaysInMonth = Contract.EndDate.Value.Day;
            }
            else
            {
                workedDaysInMonth = 30;
            }

            if (workedDaysInMonth > 30) workedDaysInMonth = 30;

            var course = db.GeClosestChangeRubToDollarOnDate(new DateTime(year, month, 15));
            var fullMonthWage = db!.GetMonthlyWage(Contract.Position!, ToursInRank, year);
            var availableMonthWageDoll = fullMonthWage * (workedDaysInMonth / 30.0m);
            var availableMonthWageRub = availableMonthWageDoll * course.Value;

            var stat = MonthlyStatistic.ContainsKey(month) ? MonthlyStatistic[month] : new ContractStatisticValue();
            stat.MaxDoll = Math.Round(availableMonthWageDoll, 2);
            stat.MaxRub = Math.Round(availableMonthWageRub, 2);
            stat.Month = GetMonth(month);

            MonthlyStatistic.TryAdd(month, stat);

            if (month == 12 && month != Contract.EndDate!.Value.Month)
            {
                month = 0;
                ++year;
            }

            MaxAvailableWageRub += stat.MaxRub;
            MaxAvailableWageDollar += stat.MaxDoll;
        }
    }

    private void InitState()
    {
        if (PassedDays >= 0)
        {
            State = PassedDays >= DurationDays ? ContractStates.Finished : ContractStates.InProgress;
        }
        else
        {
            State = ContractStates.NotStarted;
        }
    }

    public string StateString()
    {
        return State switch
        {
            ContractStates.InProgress => "В процессе",
            ContractStates.Finished => "Закончен",
            ContractStates.NotStarted => "Еще не начат",
            _ => "Неопределен"
        };
    }

    public List<decimal> GetAchivedWages()
    {
        // var result = new List<decimal>();
        // foreach (var (key,value) in MonthlyStatistic)
        // {
        //     result.Add(value.AchivedRub);
        // }
        // return result;
        return MonthlyStatistic.Select((k) => k.Value.AchivedRub).ToList();
    }

    public List<decimal> GetMaxWages()
    {
        return MonthlyStatistic.Select((k) => k.Value.MaxRub).ToList();
    }


    public override string ToString()
    {
        return $"{Contract.BeginDate} - {Contract.EndDate},{StateString()},{MonthlyStatistic.Count}";
    }

    public string GetMonth(int month)
    {
        switch (month)
        {
            case 1: return "Январь";
            case 2: return "Февраль";
            case 3: return "Март";
            case 4: return "Апрель";
            case 5: return "Май";
            case 6: return "Июнь";
            case 7: return "Июль";
            case 8: return "Август";
            case 9: return "Сентябрь";
            case 10: return "Октябрь";
            case 11: return "Ноябрь";
            case 12: return "Декабрь";
            default: return "Неопределен";
        }
    }
}

public class ContractStatisticValue()
{
    public string Month { get; set; }
    public decimal AchivedRub { get; set; } = 0.0m;
    public decimal MaxRub { get; set; } = 0.0m;
    public decimal AchivedDollar { get; set; } = 0.0m;
    public decimal MaxDoll { get; set; } = 0.0m;
}

public enum ContractStates
{
    InProgress,
    NotStarted,
    Finished
}