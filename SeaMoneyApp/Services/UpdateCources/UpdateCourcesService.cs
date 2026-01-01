using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    private readonly BehaviorSubject<bool> _canStart = new(true);
    
    public IObservable<string?> WhenErrorMessageChanged => _errorMessageSubject.AsObservable();
    
    public IObservable<bool> WhenCanStartChanged => _canStart.AsObservable();

    public async Task LoadCourcesAsync( ObservableCollection<ChangeRubToDollar> collection, 
        CancellationToken cToken = default)
    {
        _canStart.OnNext(false); // Запрет на повторный запуск
        string? errorMsg=null;
        try
        {
            cToken.ThrowIfCancellationRequested();
            _errorMessageSubject.OnNext(null); // Сброс ошибки

            var dates = GetDatesFrom2020();
            foreach (var date in dates)
            {
                if (IsDateExistInCollection(date,collection)) continue;

                var course = await HtmlParcerCbrCources.HtmlParcerCbrCources.GetUsdCourseOnDateAsync(date, cToken);
                collection.Add(course);

                LogHost.Default.Info($"Загружен курс: {course?.Date:dd.MM.yyyy} = {course?.Value:F4} ₽");
                await Task.Delay(100, cToken); // Анти-спам задержка
            }
        }
        catch (OperationCanceledException)
        {
            errorMsg = "Загрузка курсов отменена или вышло время ожидания";
            LogHost.Default.Error(errorMsg);
        }
        catch (Exception ex)
        {
            errorMsg = $"Ошибка загрузки курсов: {ex.Message}";
            LogHost.Default.Error(ex, errorMsg);
        }
        finally
        {
            if(errorMsg is not null) _errorMessageSubject.OnNext(errorMsg);
            _canStart.OnNext(true);
        }
    }
    
    public async Task<IEnumerable<ChangeRubToDollar>> UpdateCourcesAsync( ObservableCollection<ChangeRubToDollar> collection, 
        CancellationToken cToken = default)
    {
        var resultCollection = new ConcurrentQueue<ChangeRubToDollar>();
        _canStart.OnNext(false); // Запрет на повторный запуск
        string? errorMsg=null;
        try
        {
            cToken.ThrowIfCancellationRequested();
            _errorMessageSubject.OnNext(null); // Сброс ошибки

            var dates = GetDatesFrom2020();
            foreach (var date in dates)
            {
                if (IsDateExistInCollection(date,collection)) continue;
                resultCollection.Enqueue(await HtmlParcerCbrCources.HtmlParcerCbrCources.GetUsdCourseOnDateAsync(date, cToken));
                 await Task.Delay(100, cToken); // Анти-спам задержка
            }
        }
        catch (OperationCanceledException)
        {
            errorMsg = "Загрузка курсов отменена или вышло время ожидания";
            LogHost.Default.Error(errorMsg);
        }
        catch (Exception ex)
        {
            errorMsg = $"Ошибка загрузки курсов: {ex.Message}";
            LogHost.Default.Error(ex, errorMsg);
        }
        finally
        {
            if(errorMsg is not null) _errorMessageSubject.OnNext(errorMsg);
            _canStart.OnNext(true);
           
        }
        return resultCollection;
    }

    private bool IsDateExistInCollection(DateTime date, ObservableCollection<ChangeRubToDollar> collection)
    {
        var result = collection?.Any(x => x.Date == date);
        return result ?? false;
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
                month = 0;
                ++year;
            }

            ++month;
            observedDate = new DateTime(year, month, day);
        }

        return result;
    }
}