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
    private ObservableCollection<ContractStatisticValue> _selectedStatistic;

    public ObservableCollection<ContractStatisticValue> SelectedStatistic
    {
        get => _selectedStatistic;
        set => this.RaiseAndSetIfChanged(ref _selectedStatistic, value);
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
            var statisticsList =statistic.MonthlyStatistic.Values.ToList(); 
            
            SelectedStatistic = new  ObservableCollection<ContractStatisticValue>(statisticsList);
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

        Statistic = statistic;
        SelectedStatistic = new(Statistic.MonthlyStatistic.Values.ToList());
        
      
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
    }

    public Func<double, string> LabelFormatter => (value) =>
    {
        var initMonth = Statistic.MonthlyStatistic.Keys.First();
        var intValue = initMonth + value;

        if (intValue > 12) intValue -= 12;
        
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