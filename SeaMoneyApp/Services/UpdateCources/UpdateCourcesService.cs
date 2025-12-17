using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using DynamicData.Binding;
using HtmlParcerCbrCources;
using ReactiveUI;
using SeaMoneyApp.DataAccess;
using SeaMoneyApp.DataAccess.Models;
using Splat;

namespace SeaMoneyApp.Services.UpdateCources;

public class UpdateCourcesService: ReactiveObject
{
    private readonly DataBaseContext _dbContext = Locator.Current.GetService<DataBaseContext>()!;

    private readonly BehaviorSubject<string?> _errorMessageSubject = new(null);
    private readonly BehaviorSubject<bool> _canStart = new(false);

    public IObservable<string?> WhenErrorMessageChanged => _errorMessageSubject.AsObservable();
    
    public IObservable<bool> WhenCanStartChanged => _canStart.AsObservable();
    

    public async Task LoadCourcesAsync( ObservableCollection<ChangeRubToDollar> collection)
    {
        _canStart.OnNext(false); // Запрет  
        try
        {
            _errorMessageSubject.OnNext(null); // Сброс ошибки

            var dates = GetDatesFrom2020();
            foreach (var date in dates)
            {
                var course = await HTMLParcerCbrCources.GetUsdCourseOnDateAsync(date);
                if (course != null)
                {
                    collection.Add(course);
                }

                LogHost.Default.Info($"Загружен курс: {course?.Date:dd.MM.yyyy} = {course?.Value:F4} ₽");
                await Task.Delay(100); // Анти-спам задержка
            }

            // await SaveToDatabaseAsync(collection, token);
        }
        catch (Exception ex)
        {
            var errorMsg = $"Ошибка загрузки курсов: {ex.Message}";
            _errorMessageSubject.OnNext(errorMsg);
            LogHost.Default.Error(ex, errorMsg);
            _canStart.OnNext(true);
        }
        _canStart.OnNext(true);
    }

    public static List<DateTime> GetDatesFrom2020()
    {
        var result = new List<DateTime>();
        int year = 2020;
        int month = 1;
        const int day = 15;
        DateTime observedDate = new DateTime(year, month, day);
        var currentDate = DateTime.Today;

        while (observedDate <= currentDate)
        {
            result.Add(observedDate);
            if (month == 12)
            {
                month = 1;
                ++year;
            }

            ++month;
            observedDate = new DateTime(year, month, day);
        }

        return result;
    }

    public static async Task<List<ChangeRubToDollar>> GetUsdCourcesFrom2020()
    {
        var listDates = GetDatesFrom2020();
        var result = new List<ChangeRubToDollar>();
        foreach (var date in listDates)
        {
            var course = await HTMLParcerCbrCources.GetUsdCourseOnDateAsync(date);
            result.Add(course);
            LogHost.Default.Info($"Дата:{course.Date} - Курс:{course.Value}");
            Thread.Sleep(100);
        }

        return result;
    }
}