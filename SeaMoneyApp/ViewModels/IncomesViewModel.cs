using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using SeaMoneyApp.DataAccess;
using SeaMoneyApp.DataAccess.Models;
using SeaMoneyApp.Models;
using Splat;

namespace SeaMoneyApp.ViewModels;

public class IncomesViewModel : RoutableViewModel
{
    private DataBaseContext _dbContext;
    private AppSession _appSession;

    private string? _errorMessage;
    private WageLog? _selectedWageLog;
    private WageLog? _beforeEditingWageLog;
    private Contract? _selectedContract;
    private string _currentState;
    private bool _isEditing = false;
    private bool _isAdding = false;
    public ObservableCollection<WageLog> WageLogs { get; set; } = [];
    public ObservableCollection<Contract> AvailableContracts { get; set; } = [];

    public ReactiveCommand<Unit, Unit> DeleteWageLogCommand { get; set; }
    public ReactiveCommand<Unit, Unit> EditWageLogCommand { get; set; }
    public ReactiveCommand<Unit, Unit> CancelEditWageLogCommand { get; set; }
    public ReactiveCommand<Unit, Unit> SaveWageLogCommand { get; set; }
    public ReactiveCommand<Unit, Unit> AddWageLogCommand { get; set; }


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

    public WageLog? SelectedWageLog
    {
        get => _selectedWageLog;
        set => this.RaiseAndSetIfChanged(ref _selectedWageLog, value);
    }

    public WageLog? BeforeEditingWageLog
    {
        get => _beforeEditingWageLog;
        set => this.RaiseAndSetIfChanged(ref _beforeEditingWageLog, value);
    }

    public Contract? SelectedContract
    {
        get => _selectedContract;
        set => this.RaiseAndSetIfChanged(ref _selectedContract, value);
    }

    public IncomesViewModel()
    {
        LogHost.Default.Debug("IncomesViewModel старт инициализациии");

        // var t = Task.Run(InitWageLogs)
        //     .ContinueWith(async (x) => await Task.Run(() => InitCommands())).Result;


        // InitWageLogs().ContinueWith(async (x) => await Task.Run(() => InitCommands()).WaitAsync(_cts.Token));

        InitWageLogs();
        InitCommands();

        LogHost.Default.Debug("IncomesViewModel конец инициализациии");
    }

    private async Task InitWageLogs()
    {
        LogHost.Default.Debug("InitWageLogs старт инициализациии");
        _dbContext = Locator.Current.GetService<DataBaseContext>();
        _appSession = Locator.Current.GetService<AppSession>();

        _dbContext.WhenErrorMessageChanged
            .BindTo(this, vm => vm.ErrorMessage);

        var account = _appSession.CurrentAccount;

        await foreach (var wageLog in _dbContext.GetUserWageLogsAsyncEnumerable(account))
        {
            WageLogs.Add(wageLog);
        }

        await foreach (var contract in _dbContext.GetUserContractsAsyncEnumerable(account))
        {
            AvailableContracts.Add(contract);
        }
        

        if (AvailableContracts.Count == 0)
        {
            ErrorMessage = "Контракты для вашего аккаунта не найдены, сначала добавьте контракт в меню контракты";
            LogHost.Default.Warn(ErrorMessage);
        }
        else if (WageLogs.Count == 0)
        {
            ErrorMessage = "Поступления не найдены.";
        }


        LogHost.Default.Debug("InitWageLogs конец инициализациии");
    }

    private void InitCommands()
    {
        LogHost.Default.Debug("InitCommands старт инициализациии");
        DeleteWageLogCommand = ReactiveCommand.Create(DeleteWageLog, CanDeleteSelectedWageLog());
        EditWageLogCommand = ReactiveCommand.Create(EditWageLog, CanEditWageLog());
        CancelEditWageLogCommand = ReactiveCommand.Create(CancelEditingWageLog, CanCancelEditingWageLog());
        SaveWageLogCommand = ReactiveCommand.CreateFromTask(SaveWageLog, CanSaveWageLog());
        AddWageLogCommand = ReactiveCommand.Create(AddWageLog, CanAddWageLog());
        LogHost.Default.Debug("InitCommands конец инициализациии");
    }

    private void DeleteWageLog()
    {
        LogHost.Default.Debug("Попытка удаления WageLog");
        try
        {
            if (SelectedWageLog is null)
            {
                var errMsg = "Попытка удаления: SelectedWageLog равен null";
                LogHost.Default.Warn(errMsg);
                ErrorMessage = errMsg;
                return;
            }

            var searchedWageLog = WageLogs.FirstOrDefault(x => x.Date == SelectedWageLog.Date);
            if (searchedWageLog is null)
            {
                LogHost.Default.Warn("WageLog для удаления не найден в коллекции");
                return;
            }

            WageLogs.Remove(searchedWageLog);
            _dbContext.DeleteWageLog(searchedWageLog);
            SelectedContract = null;
            IsEditing = false;

            ErrorMessage = $"WageLog за {searchedWageLog.Date:dd.MM.yyyy} удалён";
            LogHost.Default.Info($"WageLog за {searchedWageLog.Date} успешно удалён");
        }
        catch (Exception ex)
        {
            LogHost.Default.Error(ex, "Ошибка при удалении WageLog");
            ErrorMessage = $"Ошибка удаления: {ex.Message}";
        }
    }

    public IObservable<bool> CanDeleteSelectedWageLog()
    {
        return this.WhenAnyValue(
            x => x.WageLogs,
            x => x.SelectedWageLog,
            x => x.IsEditing,
            (wageLogs, wageLog, adding) =>
            {
                if (wageLog is null) return false;
                if (adding) return false;
                return wageLogs.Any(c => c.Date == wageLog.Date);
            });
    }

    private void EditWageLog()
    {
        LogHost.Default.Debug("Редактирование WageLog начато");
        try
        {
            if (SelectedWageLog is null)
            {
                LogHost.Default.Warn("Попытка редактирования: SelectedWageLog равен null");
                return;
            }

            IsEditing = true;
            BeforeEditingWageLog = new WageLog(SelectedWageLog);
            CurrentState = Localization.Localization.EditingText;
            SelectedContract = AvailableContracts.FirstOrDefault(c => c.Id == SelectedWageLog.Contract.Id);

            LogHost.Default.Info($"Начато редактирование WageLog за {SelectedWageLog.Date}");
        }
        catch (Exception ex)
        {
            LogHost.Default.Error(ex, "Ошибка при старте редактирования WageLog");
            ErrorMessage = $"Ошибка редактирования: {ex.Message}";
        }
    }

    // public IObservable<bool> CanEditWageLog()
    // {
    //     return this.WhenAnyValue(
    //         x => x.IsEditing,
    //         x => x.SelectedWageLog,
    //         (editing, wageLog) => !editing && wageLog is not null);
    // }
    public IObservable<bool> CanEditWageLog()
    {
        return this.WhenAnyValue(
            x => x.IsEditing,
            x => x.SelectedWageLog,
            (editing, wageLog) => !editing && wageLog is not null);
    }

    private void CancelEditingWageLog()
    {
        LogHost.Default.Debug("Отмена редактирования WageLog");
        try
        {
            if (IsAdding)
            {
                SelectedWageLog = null;
                IsAdding = false;
                LogHost.Default.Info("Добавление WageLog отменено");
            }
            else if (BeforeEditingWageLog is not null && SelectedWageLog is not null)
            {
                var index = WageLogs.IndexOf(SelectedWageLog);
                if (index >= 0)
                {
                    WageLogs[index] = new WageLog(BeforeEditingWageLog);
                    LogHost.Default.Info($"Изменения для WageLog за {BeforeEditingWageLog.Date} отменены");
                }
            }

            BeforeEditingWageLog = null;
            IsEditing = false;
            CurrentState = Localization.Localization.ViewText;
        }
        catch (Exception ex)
        {
            LogHost.Default.Error(ex, "Ошибка при отмене редактирования WageLog");
            ErrorMessage = $"Ошибка отмены: {ex.Message}";
        }
    }

    public IObservable<bool> CanCancelEditingWageLog()
    {
        return this.WhenAnyValue(
            x => x.IsEditing,
            x => x.WageLogs,
            x => x.SelectedWageLog,
            x => x.IsAdding,
            (editing, wageLogs, wageLog, adding) =>
            {
                if (adding) return true;
                if (!editing || wageLog is null) return false;
                return wageLogs.Any(c => c.Date == wageLog.Date);
            });
    }

    private void AddWageLog()
    {
        LogHost.Default.Debug("Добавление нового WageLog начато");
        try
        {
            SelectedContract = AvailableContracts.FirstOrDefault();
            SelectedWageLog = new WageLog()
            {
                Date = DateTime.Now.Date,
                Account = _appSession.CurrentAccount,
                AmountInRub = 0.0m,
                ChangeRubToDollar = new ChangeRubToDollar(),
                Contract = SelectedContract,
                Position = _appSession.CurrentAccount.Position,
                ToursInRank = _appSession.CurrentAccount.ToursInRank,
            };
            IsAdding = true;
            IsEditing = true;
            CurrentState = Localization.Localization.AddingText;

            LogHost.Default.Info($"Новый WageLog создан с датой {SelectedWageLog.Date}");
        }
        catch (Exception ex)
        {
            LogHost.Default.Error(ex, "Ошибка при создании нового WageLog");
            ErrorMessage = $"Ошибка добавления: {ex.Message}";
        }
    }

    private IObservable<bool> CanAddWageLog()
    {
        LogHost.Default.Debug("Оценка возможности сохранения WageLog");
        return this.WhenAnyValue(x => x.IsEditing
            , x => x.AvailableContracts,
            (editing, contracts) => !editing && contracts.Count > 0);
    }

    private async Task SaveWageLog()
    {
        LogHost.Default.Debug("Сохранение изменений WageLog");
        try
        {
            SelectedWageLog.Contract = SelectedContract;
            if (IsAdding)
            {
                var notExistingCheck = WageLogs.FirstOrDefault(x =>
                    x.Date == SelectedWageLog?.Date) is null;

                if (notExistingCheck)
                {
                    var dbAddResult = await _dbContext.AddWageLogAsync(SelectedWageLog!);
                    if (dbAddResult)
                    {
                        ErrorMessage = $"Добавлен WageLog за {SelectedWageLog.Date:d}";
                        LogHost.Default.Info($"Новый WageLog за {SelectedWageLog.Date} добавлен в БД и UI");
                        var insert = new WageLog(SelectedWageLog);
                        WageLogs.Insert(0, insert);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    ErrorMessage = "WageLog на эту дату уже существует.";
                    LogHost.Default.Warn($"Попытка добавить дубликат WageLog за {SelectedWageLog?.Date}");
                    return;
                }

                IsAdding = false;
            }
            else
            {
                var succesfullUpdate = await _dbContext.UpdateWageLogAsync(BeforeEditingWageLog!, SelectedWageLog!);

                if (succesfullUpdate)
                {
                    var index = WageLogs.IndexOf(SelectedWageLog!);
                    WageLogs[index] = new WageLog(SelectedWageLog);

                    ErrorMessage = $"WageLog за {SelectedWageLog?.Date:d} сохранён";
                    LogHost.Default.Info($"WageLog за {SelectedWageLog?.Date} обновлён");
                }
                else
                {
                    return;
                }
            }

            BeforeEditingWageLog = null;
            SelectedWageLog = null;
            CurrentState = Localization.Localization.ViewText;
            IsEditing = false;
        }
        catch (Exception ex)
        {
            LogHost.Default.Error(ex, "Ошибка при сохранении WageLog");
            ErrorMessage = $"Ошибка сохранения: {ex.Message}";
        }
    }

    private IObservable<bool> CanSaveWageLog()
    {
        LogHost.Default.Debug("Оценка возможности сохранения WageLog");
        return this.WhenAnyValue(
            x => x.IsEditing,
            x => x.IsAdding,
            (isEditing, isAdding) =>
            {
                if (isAdding || isEditing) return true;
                return false;
            });
    }
}