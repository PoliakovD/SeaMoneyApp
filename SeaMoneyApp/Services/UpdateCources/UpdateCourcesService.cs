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

public class UpdateCourcesService : ReactiveObject
{
    private readonly DataBaseContext _dbContext = Locator.Current.GetService<DataBaseContext>()!;

    private readonly BehaviorSubject<string?> _errorMessageSubject = new(null);
    public readonly BehaviorSubject<bool> CanStart = new(true);

    public IObservable<string?> WhenErrorMessageChanged => _errorMessageSubject.AsObservable();

    public IObservable<bool> WhenCanStartChanged => CanStart.AsObservable();

    public void DeleteCource(ChangeRubToDollar cource) => _dbContext.DeleteChangeRubToDollar(cource);
    public void UpdateCource(ChangeRubToDollar oldCource, ChangeRubToDollar newCource) => 
        _dbContext.UpdateChangeRubToDollar(oldCource,newCource);
    public void AddCource(ChangeRubToDollar cource) => _dbContext.AddChangeRubToDollar(cource);
    
    public async IAsyncEnumerable<ChangeRubToDollar> UpdateCoursesEnumerableAsync(
        ObservableCollection<ChangeRubToDollar> collection,
        CancellationToken cToken = default)
    {
        CanStart.OnNext(false); // Запрет на повторный запуск

        cToken.ThrowIfCancellationRequested();
        _errorMessageSubject.OnNext(null); // Сброс ошибки

        var dates = GetDatesFrom2020();
        foreach (var date in dates)
        {
            if (IsDateExistInCollection(date, collection)) continue;
            var taskCourse = HtmlParcerCbrCources.HtmlParcerCbrCources.GetUsdCourseOnDateAsync(date, cToken);
            if (taskCourse is null)
            {
                var errorMsg = $"Ошибка загрузки для даты {date}";
                _errorMessageSubject.OnNext(errorMsg);
                CanStart.OnNext(true);
                throw new Exception(errorMsg);
            }
            yield return await taskCourse;
            
            await Task.Delay(100, cToken); // Анти-спам задержка
        }
        
        CanStart.OnNext(true);
        _errorMessageSubject.OnNext(null);
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