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
    private ContractStates State { get; set;}
    public Contract? Contract { get; set; }
    public double DurationDays { get; set; }
    public double PassedDays { get; set; }
    
  
    
    public double LeftDays => DurationDays - PassedDays;
    public Dictionary<int, ContractStatisticValue> MonthlyStatistic { get; set; } = [];
    public int ToursInRank { get; set; } = 0;
    public decimal CurrentAchivedWageRub { get; set; }
    public decimal CurrentAchivedWageDollar { get; set; }

    public decimal AverageCource => CurrentAchivedWageRub / CurrentAchivedWageDollar;
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

        ToursInRank = wageLogs.First().ToursInRank;

        InitState();
        InitMaxAvailableWage();
        InitCurrentAchivedWage(wageLogs);
        
        
    }
    
    private void InitCurrentAchivedWage(IEnumerable<WageLog> wageLogs)
    {
        var contractWages = wageLogs.Where(x => x.Contract!.Id == Contract!.Id);
        if (contractWages is null) return;
        foreach (var wageLog in contractWages)
        {
            var received = wageLog.AmountInRub;
            var month = wageLog.Date!.Value.Month;
            
            
            var stat = MonthlyStatistic.ContainsKey(month) ? MonthlyStatistic[month] : new ContractStatisticValue();
            stat.AchivedDollar += wageLog.AmountInDollars!.Value;
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
        for (int month = Contract.BeginDate.Month; month != Contract.EndDate!.Value.Month; month++)
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
            var availableMonthWageDoll = fullMonthWage * ((decimal)workedDaysInMonth / daysInMonth);
            var availableMonthWageRub = availableMonthWageDoll * course.Value;
            
            var stat = MonthlyStatistic.ContainsKey(month) ? MonthlyStatistic[month] : new ContractStatisticValue();
            stat.MaxDoll = Math.Round(availableMonthWageDoll,2);
            stat.MaxRub =  Math.Round(availableMonthWageRub,2); 
            
            MonthlyStatistic.TryAdd(month, stat);

            if (month == 12)
            {
                month = 0;
                ++year;
            }
            MaxAvailableWageRub+=stat.MaxRub;
            MaxAvailableWageDollar += stat.MaxDoll ;
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
}

public record ContractStatisticValue()
{
    public decimal AchivedRub = 0.0m;
    public decimal AchivedDollar = 0.0m;
    public decimal MaxRub = 0.0m;
    public decimal MaxDoll = 0.0m;
}

public class ContractStatisticDurationPieData
{
    public string Name { get; set; } = "";
    public double[] Values { get; set; } = [];
}

public enum ContractStates
{
    InProgress,
    NotStarted,
    Finished
}