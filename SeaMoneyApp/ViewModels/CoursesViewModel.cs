using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using ReactiveUI;
using SeaMoneyApp.DataAccess;
using SeaMoneyApp.DataAccess.Models;
using SeaMoneyApp.Services.UpdateCources;
using Splat;

namespace SeaMoneyApp.ViewModels;

public partial class CoursesViewModel : RoutableViewModel
{
    private const int HttpTimeOut = 100000;
    public ObservableCollection<ChangeRubToDollar> Courses { get; set; } = [];

    private readonly DataBaseContext _dbContext = Locator.Current.GetService<DataBaseContext>()!;

    private string? _errorMessage;

    // private ChangeRubToDollar _selectedCourse = new();
    private ChangeRubToDollar? _selectedCourse;
    private ChangeRubToDollar? _beforeEditingCourse;
    private string _currentState;
    private bool _isEditing;

    private CancellationTokenSource _ctsHttp;
    public ReactiveCommand<Unit, Unit> UpdateCoursesFromHttpCommand { get; }
    public ReactiveCommand<Unit, Unit> StopLoadFromHttpCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteCourseCommand { get; }

    public ReactiveCommand<Unit, Unit> EditCourseCommand { get; }

    public ReactiveCommand<Unit, Unit> CancelEditCourseCommand { get; }

    public ReactiveCommand<Unit, Unit> SaveCourseCommand { get; }

    public ReactiveCommand<Unit, Unit> AddCourseCommand { get; }

    public ReactiveCommand<Unit, Unit> SaveAddedCourseCommand { get; }

    public IObservable<bool> htmlRunning
    {
        get { return _htmlRunning; }
        set { _htmlRunning = value; }
    }

    public IObservable<bool> CanDeleteSelectedCourse { get; set; }

    public ChangeRubToDollar? SelectedCourse
    {
        get => _selectedCourse;
        set => this.RaiseAndSetIfChanged(ref _selectedCourse, value);
    }

    public ChangeRubToDollar? BeforeEditingCourse
    {
        get => _beforeEditingCourse;
        set => this.RaiseAndSetIfChanged(ref _beforeEditingCourse, value);
    }

    public string CurrentState
    {
        get => _currentState;
        set => this.RaiseAndSetIfChanged(ref _currentState, value);
    }

    public bool CanSelectAnotherCource { get; set; } = true;
    public bool CanAddAnotherCourse { get; set; } = true;

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    public bool IsEditing
    {
        get => _isEditing;
        private set => this.RaiseAndSetIfChanged(ref _isEditing, value);
    }

    public bool IsAdding
    {
        get => _isAdding;
        private set => this.RaiseAndSetIfChanged(ref _isAdding, value);
    }

    private readonly UpdateCourcesService _updateService;
    private IObservable<bool> _canDeleteSelectedCourse;
    private IObservable<bool> _htmlRunning;
    private bool _isAdding;

    public CoursesViewModel()
    {
        // Инициализация Chart
        ChartInit();
        // загрузка курсов из бд
        LoadFromDb();
        IsEditing = false;
        IsAdding = false;
        SelectedCourse = new();

        _updateService = Locator.Current.GetService<UpdateCourcesService>()
                         ?? throw new ArgumentNullException(nameof(_updateService));

        htmlRunning = _updateService.WhenCanStartChanged;


        UpdateCoursesFromHttpCommand = ReactiveCommand.CreateFromTask(
            UpdateEnumerableCourcesFromHttpAsync,
            canExecute: htmlRunning
        );


        StopLoadFromHttpCommand = ReactiveCommand.Create(() => _ctsHttp?.Cancel(),
            canExecute: UpdateCoursesFromHttpCommand.IsExecuting);

        AddCourseCommand = ReactiveCommand.Create(AddCourse,
            canExecute: this.WhenAnyValue(x => x.CanAddAnotherCourse));

        CanDeleteSelectedCourse = this.WhenAnyValue(x => x.Courses, x => x.SelectedCourse,
            (cources, cource) =>
            {
                if (cource is null) return false;
                foreach (var c in cources)
                {
                    if (c.Date == cource.Date) return true;
                }

                return false;
            });

        // CancelEditCourceCommand =
        //     ReactiveCommand.Create(CancelEditingCourse,
        //         this.WhenAnyValue(x => x.IsEditing, (x) => x == true));

        CancelEditCourseCommand =
            ReactiveCommand.Create(CancelEditingCourse,
                this.WhenAnyValue(
                    x => x.IsEditing,
                    x => x.Courses,
                    x => x.SelectedCourse,
                    x => x.IsAdding,
                    (editing, cources, cource, adding) =>
                    {
                        if (adding) return true;
                        if (cource is null) return false;
                        foreach (var c in cources)
                        {
                            if (editing && c.Date == cource.Date) return true;
                        }

                        return false;
                    }));

        DeleteCourseCommand = ReactiveCommand.Create(DeleteCourse, CanDeleteSelectedCourse);

        EditCourseCommand = ReactiveCommand.Create(EditCourse,
            this.WhenAnyValue(
                x => x.IsEditing,
                c => c.SelectedCourse,
                (edit, cource) => !edit && cource is not null));

        SaveCourseCommand = ReactiveCommand.Create(SaveCourse, CanSaveCourse());

        AddCourseCommand = ReactiveCommand.Create(AddCourse, this.WhenAnyValue(
            x => x.IsEditing,
            x => !x));

        // Подписываемся на изменения ошибки
        _updateService.WhenErrorMessageChanged
            .BindTo(this, vm => vm.ErrorMessage);
        CurrentState = "Просмотр";
    }


    private async Task UpdateEnumerableCourcesFromHttpAsync()
    {
        _ctsHttp = new CancellationTokenSource(HttpTimeOut);
        var cToken = _ctsHttp.Token;
        try
        {
            cToken.ThrowIfCancellationRequested();
            LogHost.Default.Debug("UpdateCourcesFromHttpCommand started");

            var counter = 0;

            await foreach (var course in _updateService.UpdateCourcesEnumerableAsync(Courses, cToken))
            {
                //Cources.Add(course);
                Courses.Insert(0, course);
                _dbContext.ChangeRubToDollars.Add(course);
                counter++;
                ErrorMessage = $"Добавлено {counter} новых курсов.";
                MaxX = MaxX > course.Date.Ticks ? MaxX : course.Date.Ticks;
            }

            if (counter > 0)
            {
                _dbContext.SaveChanges();
                ErrorMessage = $"Всего добавлено {counter} новых курсов.";
            }
            else
            {
                ErrorMessage = $"Все курсы соответствуют текущим";
            }

            LogHost.Default.Debug("UpdateCourcesFromHttpCommand finished");
        }
        catch (OperationCanceledException)
        {
            LogHost.Default.Debug("UpdateCourcesFromHttpCommand cancelled or timed out");
        }
        catch (Exception ex)
        {
            LogHost.Default.Error(ex, "UpdateCourcesFromHttpCommand error");
        }
    }

    private void LoadFromDb()
    {
        try
        {
            long min = Int64.MaxValue;
            long max = 0L;
            var courses = _dbContext.GetAllCources();
            foreach (var course in courses)
            {
                Courses.Insert(0, course);
            }

            if (courses.Count > 1)
            {
                min = courses[0].Date.Ticks;
                max = courses.Last().Date.Ticks;
            }


            MinX = min;
            MaxX = max;
        }
        catch (Exception e)
        {
            LogHost.Default.Error(e.Message);
            throw;
        }
    }

    private void AddCourse()
    {
        try
        {
            var course = new ChangeRubToDollar()
            {
                Date = DateTime.Now,
                Value = 0.0m
            };
            SelectedCourse = course;
            IsAdding = true;
            IsEditing = true;
            CurrentState = "Добавление";
            CanSelectAnotherCource = false;
        }
        catch (Exception e)
        {
            LogHost.Default.Error(e.Message);
            throw;
        }
    }


    private void DeleteCourse()
    {
        try
        {
            if (SelectedCourse is null)
            {
                LogHost.Default.Debug("SelectedCourse cource is null");
                return;
            }

            var searchedCourse = Courses.FirstOrDefault(x => x.Date == SelectedCourse.Date);
            LogHost.Default.Debug("DeleteCource started");
            if (searchedCourse is null)
            {
                LogHost.Default.Debug("searched cource is null");
                return;
            }

            ErrorMessage = $"Удален Курс за {searchedCourse.Date:dd.MM.yyyy}";
            Courses.Remove(searchedCourse);
            _updateService.DeleteCource(searchedCourse);
            SelectedCourse = null;
            IsEditing = false;
            CanAddAnotherCourse = true;
        }
        catch (Exception e)
        {
            LogHost.Default.Error(e.Message);
            throw;
        }
    }

    private void EditCourse()
    {
        try
        {
            IsEditing = true;
            _beforeEditingCourse = new ChangeRubToDollar()
            {
                Id = SelectedCourse!.Id,
                Date = SelectedCourse!.Date,
                Value = SelectedCourse!.Value
            };
            CurrentState = "Редактирование";
        }
        catch (Exception e)
        {
            LogHost.Default.Error(e.Message);
            throw;
        }
    }

    private void CancelEditingCourse()
    {
        try
        {
            if (IsAdding)
            {
                SelectedCourse = null;
                IsAdding = false;
            }
            else
            {
                var index = Courses.IndexOf(SelectedCourse!);
                Courses[index] = new ChangeRubToDollar()
                {
                    Id = _beforeEditingCourse!.Id,
                    Date = _beforeEditingCourse.Date,
                    Value = _beforeEditingCourse.Value
                };
                _beforeEditingCourse = null;
            }

            IsEditing = false;
            CurrentState = "Просмотр";
        }
        catch (Exception e)
        {
            LogHost.Default.Error(e.Message);
            throw;
        }
    }

    private void SaveCourse()
    {
        try
        {
            if (IsAdding)
            {
                var flag = Courses.FirstOrDefault(x =>
                    x.Date.Day == SelectedCourse?.Date.Day &&
                    x.Date.Month == SelectedCourse?.Date.Month &&
                    x.Date.Year == SelectedCourse?.Date.Year);
                if (flag is null)
                {
                    _updateService.AddCource(SelectedCourse!);
                    Courses.Add(new ChangeRubToDollar
                    {
                        Date = SelectedCourse!.Date,
                        Value = SelectedCourse.Value
                    });
                    ErrorMessage = $"Добавлен курс за {SelectedCourse.Date:d} число";
                }
                else
                {
                    ErrorMessage = "Такая дата уже есть в базе.";
                }
                IsAdding = false;
            }
            else
            {
                _updateService.UpdateCource(_beforeEditingCourse!, SelectedCourse!);

                var index = Courses.IndexOf(SelectedCourse!);
                Courses[index] = new ChangeRubToDollar()
                {
                    Id = SelectedCourse!.Id,
                    Date = SelectedCourse.Date,
                    Value = SelectedCourse.Value
                };
                ErrorMessage = $"Сохранен курс за {Courses[index].Date:d} число";
            }

            _beforeEditingCourse = null;
            SelectedCourse = null;
            CurrentState = "Просмотр";
            IsEditing = false;
        }
        catch (Exception e)
        {
            LogHost.Default.Error(e.Message);
            throw;
        }
    }

    private IObservable<bool> CanSaveCourse()
    {
        try
        {
            return this.WhenAnyValue(
                b => b.BeforeEditingCourse,
                a => a.SelectedCourse,
                e => e.IsEditing,
                a => a.IsAdding,
                c => c.Courses,
                (before, after, isediting, isadding, courses) =>
                {
                    if (isadding)
                    {
                        return isadding;
                    }
                    else
                    {
                        return isediting && (before?.Value != after?.Value || before?.Date != after?.Date);
                    }
                });
        }
        catch (Exception e)
        {
            LogHost.Default.Error(e.Message);
            throw;
        }
    }
}