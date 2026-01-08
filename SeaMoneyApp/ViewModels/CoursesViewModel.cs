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
    private ChangeRubToDollar? _selectedCourse;
    private ChangeRubToDollar? _beforeEditingCourse;
    private string _currentState;
    private bool _isEditing;
    private bool _isAdding;

    private CancellationTokenSource _ctsHttp;
    public ReactiveCommand<Unit, Unit> UpdateCoursesFromHttpCommand { get; set; }
    public ReactiveCommand<Unit, Unit> StopLoadFromHttpCommand { get; set; }
    public ReactiveCommand<Unit, Unit> DeleteCourseCommand { get;set;  }
    public ReactiveCommand<Unit, Unit> EditCourseCommand { get;set;  }
    public ReactiveCommand<Unit, Unit> CancelEditCourseCommand { get; set; }
    public ReactiveCommand<Unit, Unit> SaveCourseCommand { get;set;  }
    public ReactiveCommand<Unit, Unit> AddCourseCommand { get;set;  }

    public IObservable<bool> HtmlRunning
    {
        get => _htmlRunning;
        set => this.RaiseAndSetIfChanged(ref _htmlRunning, value);
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

    private UpdateCourcesService _updateService;
    private IObservable<bool> _canDeleteSelectedCourse;
    private IObservable<bool> _htmlRunning;
    

    public CoursesViewModel()
    {
        Task.Run(() =>
        {
            LogHost.Default.Debug("CoursesViewModel начальная инициализация");

        // Инициализация Chart
        ChartInit();
        // Загрузка курсов из БД
        LoadFromDb();

        IsEditing = false;
        IsAdding = false;
        SelectedCourse = new();

        _updateService = Locator.Current.GetService<UpdateCourcesService>()
                         ?? throw new ArgumentNullException(nameof(_updateService));

        HtmlRunning = _updateService.WhenCanStartChanged;

        UpdateCoursesFromHttpCommand = ReactiveCommand.CreateFromTask(
            UpdateEnumerableCourcesFromHttpAsync,
            canExecute: HtmlRunning
        );

        StopLoadFromHttpCommand = ReactiveCommand.Create(
            () => _ctsHttp?.Cancel(),
            canExecute: UpdateCoursesFromHttpCommand.IsExecuting);

        CanDeleteSelectedCourse = this.WhenAnyValue(
            x => x.Courses, 
            x => x.SelectedCourse,
            x=>x.IsEditing,
            (courses, course,adding) =>
            {
                if (course is null) return false;
                if (adding) return false;
                return courses.Any(c => c.Date.Date == course.Date.Date);
            });

        CancelEditCourseCommand = ReactiveCommand.Create(
            CancelEditingCourse,
            this.WhenAnyValue(
                x => x.IsEditing,
                x => x.Courses,
                x => x.SelectedCourse,
                x => x.IsAdding,
                (editing, courses, course, adding) =>
                {
                    if (adding) return true;
                    if (!editing || course is null) return false;
                    return courses.Any(c => c.Date.Date == course.Date.Date);
                }));

        DeleteCourseCommand = ReactiveCommand.Create(DeleteCourse, CanDeleteSelectedCourse);

        EditCourseCommand = ReactiveCommand.Create(
            EditCourse,
            this.WhenAnyValue(
                x => x.IsEditing,
                x => x.SelectedCourse,
                (editing, course) => !editing && course is not null));

        SaveCourseCommand = ReactiveCommand.Create(SaveCourse, CanSaveCourse());

        AddCourseCommand = ReactiveCommand.Create(
            AddCourse,
            this.WhenAnyValue(x => x.IsEditing, x => !x));

        _updateService.WhenErrorMessageChanged
            .BindTo(this, vm => vm.ErrorMessage);

        CurrentState = Localization.Localization.ViewText;

        LogHost.Default.Debug("CoursesViewModel инициализация завершена");
        });
        
    }

    private async Task UpdateEnumerableCourcesFromHttpAsync()
    {
        LogHost.Default.Debug("Команда обновления курсов из HTTP запущена");
        _ctsHttp = new CancellationTokenSource(HttpTimeOut);
        var cToken = _ctsHttp.Token;

        try
        {
            cToken.ThrowIfCancellationRequested();

            var counter = 0;
            var asyncCourses = _updateService.UpdateCoursesEnumerableAsync(Courses, cToken);
            if (asyncCourses is null)
            {
                LogHost.Default.Error("Ошибка при загрузке курсов из HTTP");
                ErrorMessage = $"Ошибка загрузки: ";
                return;
            }
            await foreach (var course in asyncCourses)
            {
                Courses.Insert(0, course);
                _dbContext.ChangeRubToDollars.Add(course);
                counter++;
                MaxX = Math.Max(MaxX, course.Date.Ticks);

                LogHost.Default.Debug($"Добавлен курс: {course.Date}, значение: {course.Value}");
                ErrorMessage = ($"Добавлено {counter} новых курсов");
            }

            if (counter > 0)
            {
                _dbContext.SaveChanges();
                ErrorMessage = $"Всего добавлено {counter} новых курсов.";
                LogHost.Default.Info($"Сохранено {counter} новых курсов в БД");
            }
            else
            {
                ErrorMessage = "Все курсы уже актуальны.";
                LogHost.Default.Info("Обновление завершено: новых данных нет");
            }
        }
        catch (OperationCanceledException)
        {
            LogHost.Default.Warn("Загрузка курсов отменена или истекло время ожидания");
            ErrorMessage = "Загрузка отменена или таймаут.";
        }
        catch (Exception ex)
        {
            LogHost.Default.Error(ex, "Ошибка при загрузке курсов из HTTP");
            _updateService.CanStart.OnNext(true);
            ErrorMessage = $"Ошибка загрузки: {ex.Message}";
        }
    }

    private void LoadFromDb()
    {
        LogHost.Default.Debug("Начало загрузки курсов из базы данных");
        try
        {
            long min = long.MaxValue;
            long max = 0;

            var courses = _dbContext.GetAllCources();
            LogHost.Default.Info($"Загружено {courses.Count} курсов из БД");

            foreach (var course in courses)
            {
                Courses.Insert(0, course);
            }

            if (courses.Count > 0)
            {
                min = courses.Min(c => c.Date.Ticks);
                max = courses.Max(c => c.Date.Ticks);
            }

            MinX = min;
            MaxX = max;

            LogHost.Default.Debug($"Загрузка из БД завершена. Диапазон: {min} — {max}");
        }
        catch (Exception ex)
        {
            LogHost.Default.Error(ex, "Ошибка при загрузке данных из базы");
            ErrorMessage = $"Ошибка БД: {ex.Message}";
        }
    }

    private void AddCourse()
    {
        LogHost.Default.Debug("Добавление нового курса начато");
        try
        { 
            SelectedCourse = new ChangeRubToDollar
            {
                Date = DateTime.Now,
                Value = 0.0m
            };
            IsAdding = true;
            IsEditing = true;
            CurrentState = Localization.Localization.AddingText;
            
            LogHost.Default.Info($"Новый курс создан с датой {SelectedCourse.Date}");
        }
        catch (Exception ex)
        {
            LogHost.Default.Error(ex, "Ошибка при создании нового курса");
            ErrorMessage = $"Ошибка добавления: {ex.Message}";
        }
    }

    private void DeleteCourse()
    {
        LogHost.Default.Debug("Попытка удаления курса");
        try
        {
            if (SelectedCourse is null)
            {
                LogHost.Default.Warn("Попытка удаления: SelectedCourse равен null");
                return;
            }

            var searchedCourse = Courses.FirstOrDefault(x => x.Date.Date == SelectedCourse.Date.Date);
            if (searchedCourse is null)
            {
                LogHost.Default.Warn("Курс для удаления не найден в коллекции");
                return;
            }

            Courses.Remove(searchedCourse);
            _updateService.DeleteCource(searchedCourse);
            SelectedCourse = null;
            IsEditing = false;
           
            ErrorMessage = $"Курс за {searchedCourse.Date:dd.MM.yyyy} удалён";
            LogHost.Default.Info($"Курс за {searchedCourse.Date} успешно удалён");
        }
        catch (Exception ex)
        {
            LogHost.Default.Error(ex, "Ошибка при удалении курса");
            ErrorMessage = $"Ошибка удаления: {ex.Message}";
        }
    }

    private void EditCourse()
    {
        LogHost.Default.Debug("Редактирование курса начато");
        try
        {
            if (SelectedCourse is null)
            {
                LogHost.Default.Warn("Попытка редактирования: SelectedCourse равен null");
                return;
            }

            IsEditing = true;
            _beforeEditingCourse = new ChangeRubToDollar
            {
                Id = SelectedCourse.Id,
                Date = SelectedCourse.Date,
                Value = SelectedCourse.Value
            };
            CurrentState = Localization.Localization.EditingText;

            LogHost.Default.Info($"Начато редактирование курса за {SelectedCourse.Date}");
        }
        catch (Exception ex)
        {
            LogHost.Default.Error(ex, "Ошибка при старте редактирования курса");
            ErrorMessage = $"Ошибка редактирования: {ex.Message}";
        }
    }

    private void CancelEditingCourse()
    {
        LogHost.Default.Debug("Отмена редактирования курса");
        try
        {
            if (IsAdding)
            {
                SelectedCourse = null;
                IsAdding = false;
                LogHost.Default.Info("Добавление курса отменено");
            }
            else if (_beforeEditingCourse is not null && SelectedCourse is not null)
            {
                var index = Courses.IndexOf(SelectedCourse);
                if (index >= 0)
                {
                    Courses[index] = new ChangeRubToDollar
                    {
                        Id = _beforeEditingCourse.Id,
                        Date = _beforeEditingCourse.Date,
                        Value = _beforeEditingCourse.Value
                    };
                    LogHost.Default.Info($"Изменения для курса за {_beforeEditingCourse.Date} отменены" );
                }
            }

            _beforeEditingCourse = null;
            IsEditing = false;
            CurrentState = Localization.Localization.ViewText;
        }
        catch (Exception ex)
        {
            LogHost.Default.Error(ex, "Ошибка при отмене редактирования курса");
            ErrorMessage = $"Ошибка отмены: {ex.Message}";
        }
    }

    private void SaveCourse()
    {
        LogHost.Default.Debug("Сохранение изменений курса");
        try
        {
            if (IsAdding)
            {
                var existing = Courses.FirstOrDefault(x =>
                    x.Date.Date == SelectedCourse?.Date.Date);

                if (existing is null)
                {
                    _updateService.AddCource(SelectedCourse!);
                    Courses.Insert(0,new ChangeRubToDollar
                    {
                        Date = SelectedCourse!.Date,
                        Value = SelectedCourse.Value
                    });
                    ErrorMessage = $"Добавлен курс за {SelectedCourse.Date:d}";
                    LogHost.Default.Info($"Новый курс за {SelectedCourse.Date} добавлен в БД и UI" );
                }
                else
                {
                    ErrorMessage = "Курс на эту дату уже существует.";
                    LogHost.Default.Warn($"Попытка добавить дубликат курса за {SelectedCourse?.Date}" );
                }

                IsAdding = false;
            }
            else
            {
                _updateService.UpdateCource(_beforeEditingCourse!, SelectedCourse!);

                if (Courses.Contains(SelectedCourse))
                {
                    var index = Courses.IndexOf(SelectedCourse!);
                    Courses[index] = new ChangeRubToDollar
                    {
                        Id = SelectedCourse!.Id,
                        Date = SelectedCourse.Date,
                        Value = SelectedCourse.Value
                    };

                    ErrorMessage = $"Курс за {SelectedCourse?.Date:d} сохранён";
                    LogHost.Default.Info($"Курс за {SelectedCourse?.Date} обновлён" );
                }
                else
                {
                    LogHost.Default.Error("Ошибка при сохранении курса");
                    ErrorMessage = "Ошибка Сохранения курса";
                }
            }

            _beforeEditingCourse = null;
            SelectedCourse = null;
            CurrentState = Localization.Localization.ViewText;
            IsEditing = false;
        }
        catch (Exception ex)
        {
            LogHost.Default.Error(ex, "Ошибка при сохранении курса");
            ErrorMessage = $"Ошибка сохранения: {ex.Message}";
        }
    }

    private IObservable<bool> CanSaveCourse()
    {
        LogHost.Default.Debug("Оценка возможности сохранения курса");
        return this.WhenAnyValue(
            x => x.BeforeEditingCourse,
            x => x.SelectedCourse,
            x => x.IsEditing,
            x => x.IsAdding,
            x => x.Courses,
            (before, after, isEditing, isAdding, courses) =>
            {
                if (isAdding)
                {
                    return true;
                }

                return isEditing &&
                       (before?.Value != after?.Value || before?.Date.Date != after?.Date.Date);
            });
    }
}