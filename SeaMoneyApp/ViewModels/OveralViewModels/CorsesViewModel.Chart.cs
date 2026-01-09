using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Events;
using LiveChartsCore.Kernel.Sketches;
using ReactiveUI;
using SeaMoneyApp.DataAccess.Models;
using Splat;

namespace SeaMoneyApp.ViewModels.OveralViewModels;

public partial class CoursesViewModel
{
    private static double _defaultMinX = 0.0;
    private static double _defaultMaxX = 100.0;
    private bool _isDown = false;
    private double _minX;
    private double _maxX;

    private double _minXThumb;
    private double _maxXThumb;


    public ReactiveCommand<ChartCommandArgs, Unit> ChartUpdatedCommand { get; private set; }
    public ReactiveCommand<PointerCommandArgs, Unit> PointerDownCommand { get; private set; }
    public ReactiveCommand<PointerCommandArgs, Unit> PointerMoveCommand { get; private set; }
    public ReactiveCommand<PointerCommandArgs, Unit> PointerUpCommand { get; private set; }

    public ObservableCollection<DateTimePoint?> Values { get; set; } = [];

    public double MinX
    {
        get => _minX;
        private set => this.RaiseAndSetIfChanged(ref _minX, value);
    }

    public double MaxX
    {
        get => _maxX;
        private set => this.RaiseAndSetIfChanged(ref _maxX, value);
    }


    public double MinXThumb
    {
        get => _minXThumb;
        private set => this.RaiseAndSetIfChanged(ref _minXThumb, value);
    }

    public double MaxXThumb
    {
        get => _maxXThumb;
        private set => this.RaiseAndSetIfChanged(ref _maxXThumb, value);
    }

    public void ChartUpdated(ChartCommandArgs args)
    {
        var cartesianChart = (CartesianChartEngine)args.Chart.CoreChart;
        var x = cartesianChart.XAxes.First();

        // when the main chart is updated, we need to update the scroll bar thumb limits
        // this will sync the scroll bar with the main chart when the user is zooming or panning

        MinXThumb = x.MinLimit ?? _defaultMinX;
        MaxXThumb = x.MaxLimit ?? _defaultMaxX;
    }

    public void PointerDown(PointerCommandArgs args) =>
        _isDown = true;

    public void PointerMove(PointerCommandArgs args)
    {
        if (!_isDown) return;

        var chart = (ICartesianChartView)args.Chart;
        var positionInData = chart.ScalePixelsToData(args.PointerPosition);

        var currentRangeX = MaxXThumb - MinXThumb;

        //var currentRangeY = MaxYThumb - MinYThumb;

        var minX = positionInData.X - currentRangeX / 2;
        var maxX = positionInData.X + currentRangeX / 2;


        // data bounds as limits for the thumb

        if (Values[0] is not null && Values[1] is not null)
        {
            if (minX < Values[0].Coordinate.SecondaryValue)
            {
                minX = Values[0].Coordinate.SecondaryValue;
                maxX = minX + currentRangeX;
            }

            if (maxX > Values[^1].Coordinate.SecondaryValue)
            {
                maxX = Values[^1].Coordinate.SecondaryValue;
                minX = maxX - currentRangeX;
            }

            // update the scroll bar thumb when the user is dragging the chart
            MinXThumb = minX;
            MaxXThumb = maxX;


            // update the chart visible range
            MinX = minX;
            MaxX = maxX;
        }
    }

    public void PointerUp(PointerCommandArgs args) =>
        _isDown = false;

    private void ChartInit()
    {
        MinXThumb = _defaultMinX;
        MaxXThumb = _defaultMaxX;


        ChartUpdatedCommand = ReactiveCommand.Create<ChartCommandArgs>(ChartUpdated);
        PointerDownCommand = ReactiveCommand.Create<PointerCommandArgs>(PointerDown);
        PointerMoveCommand = ReactiveCommand.Create<PointerCommandArgs>(PointerMove);
        PointerUpCommand = ReactiveCommand.Create<PointerCommandArgs>(PointerUp);

        Courses.CollectionChanged += OnCollectionChanged;
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        switch (eventArgs.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (var item in eventArgs.NewItems)
                {
                    var cource = (ChangeRubToDollar)item;
                    if (cource is null) break;
                    var point = new DateTimePoint(cource.Date, Convert.ToDouble(cource.Value));
                    Values.Add(point);
                    //Values.Insert(0,point);
                }

                break;
            case NotifyCollectionChangedAction.Remove:
            {
                foreach (var item in eventArgs.OldItems)
                {
                    var cource = (ChangeRubToDollar)item;
                    var point = Values.FirstOrDefault(p => p.DateTime == cource.Date);
                    Values.Remove(point);
                }

                break;
            }
            case NotifyCollectionChangedAction.Replace:
            {
                try
                {
                    var oldItem = (eventArgs.OldItems![0] as ChangeRubToDollar)!;
                    var newItem = (eventArgs.NewItems![0] as ChangeRubToDollar)!;
                    var oldPoint = Values.FirstOrDefault(p => p.DateTime == oldItem.Date);
                    
                    int index = Values.IndexOf(oldPoint);
                    Values[index] = new DateTimePoint(newItem.Date, Convert.ToDouble(newItem.Value));
                }
                catch (Exception e)
                {
                    LogHost.Default.Error(e.Message);
                }
                break;
            }

            case NotifyCollectionChangedAction.Reset:
            {
                Values.Clear();
            }
                break;
        }
    }

    public static Func<double, string> DateTimeFormat =>
        value => Convert.ToDateTime(value).ToString("mm.yyyy");
}