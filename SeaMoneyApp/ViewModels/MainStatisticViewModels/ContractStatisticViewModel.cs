using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using DynamicData;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using ReactiveUI;
using SeaMoneyApp.Models;
using Splat;

namespace SeaMoneyApp.ViewModels.MainStatisticViewModels;

public class ContractStatisticViewModel : RoutableViewModel
{
    private IEnumerable _selectedData;

    public IEnumerable SelectedData
    {
        get => _selectedData;
        set => this.RaiseAndSetIfChanged(ref _selectedData, value);
    }

    public ContractStatistic Statistic
    {
        get => _statistic;
        set => this.RaiseAndSetIfChanged(ref _statistic, value);
    }

    public ISeries[] PieDurationSeries { get; set; }
    public List<double> PassedDaysPie => [Statistic.PassedDays];
    public List<double> LeftDaysPie => [Statistic.LeftDays];

    public List<decimal> AchivedWages => Statistic.GetAchivedWages();
    public List<decimal> MaxWages => Statistic.GetMaxWages();
    private ISeries[] _barWageSeries;
    private ContractStatistic _statistic;

    public ISeries[] BarWageSeries
    {
        get => _barWageSeries;
        set => this.RaiseAndSetIfChanged(ref _barWageSeries, value);
    }

    public ContractStatisticViewModel(ContractStatistic statistic)
    {
        try
        {
            Init(statistic);
            SelectedData = Statistic.MonthlyStatistic.Values;
        }
        catch (Exception e)
        {
            LogHost.Default.Error(e.Message);
            LogHost.Default.Error(e.StackTrace);
            throw;
        }
    }

    private void Init(ContractStatistic statistic)
    {
        // Statistic = statistic;
        // PieDurationSeries =
        // [
        //     new PieSeries<double> { Name = "Прошло", Values = PassedDaysPie },
        //     new PieSeries<double> { Name = "Осталось", Values = LeftDaysPie }
        // ];
        // var cs1 = new ColumnSeries<decimal>()
        // {
        //     Name = "Получено",
        //     Values = TestValues
        // };
        // // LogHost.Default.Error($"{cs1.Name}, {cs1.Values.First()}");
        // BarWageSeries = new ISeries[1];
        // BarWageSeries[0] = cs1;

        Statistic = statistic;
        PieDurationSeries =
        [
            new PieSeries<double> { Name = "Прошло", Values = PassedDaysPie },
            new PieSeries<double> { Name = "Осталось", Values = LeftDaysPie }
        ];
        BarWageSeries =
        [
            new ColumnSeries<decimal>
            {
                Name = "Получено",
                Values = AchivedWages
            },
            new ColumnSeries<decimal>
            {
                Name = "Расчетная",
                Values = MaxWages
            }
        ];
        // LogHost.Default.Error($"{cs1.Name}, {cs1.Values.First()}");


        // LogHost.Default.Info($"{BarWageSeries.Length}");
        // int counter = 0;
        // foreach (var (key, statisticValue) in Statistic.MonthlyStatistic)
        // {
        //     LogHost.Default.Info(counter.ToString());
        //     LogHost.Default.Info(statisticValue.ToString());
        //     BarWageSeries[counter] = new  ColumnSeries<decimal>
        //     {
        //         Name = key.ToString(),
        //         // Values = [statisticValue.AchivedRub,statisticValue.MaxRub]
        //         Values = [statisticValue.AchivedRub]
        //     };
        //     counter++;
        // }
    }

    public Func<double, string> LabelFormatter => (value) =>
    {
        if (value <0) return "00000000";
        if (value > Statistic.MonthlyStatistic.Keys.Count -1) return "9999999999";
        var intValue = Statistic.MonthlyStatistic.Keys.ToList() [(int)value];
        switch (intValue)
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
    };
}