using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using SeaMoneyApp.DataAccess;
using SeaMoneyApp.DataAccess.Models;
using SeaMoneyApp.Models;
using Splat;

namespace SeaMoneyApp.ViewModels.OverallViewModels;

public class ContractsViewModel : RoutableViewModel
{
    public ObservableCollection<Contract> UserContracts { get; set; } = [];
    private DataBaseContext _dbContext;
    private AppSession _appSession;


    private string? _errorMessage;
    private Contract? _selectedContract;
    private Contract? _beforeEditingContract;
    private string _currentState;
    private bool _isEditing;
    private bool _isAdding;
    private IEnumerable<Contract?> _wageLogs;

    public ReactiveCommand<Unit, Unit> DeleteContractCommand { get; set; }
    public ReactiveCommand<Unit, Unit> EditContractCommand { get; set; }
    public ReactiveCommand<Unit, Unit> CancelEditContractCommand { get; set; }
    public ReactiveCommand<Unit, Unit> SaveContractCommand { get; set; }
    public ReactiveCommand<Unit, Unit> AddContractCommand { get; set; }
    public IObservable<bool> CanDeleteSelectedContract { get; set; }

    public string CurrentState
    {
        get => _currentState;
        set => this.RaiseAndSetIfChanged(ref _currentState, value);
    }

    public IEnumerable<Contract?> WageLogs
    {
        get => _wageLogs;
        set => this.RaiseAndSetIfChanged(ref _wageLogs, value);
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

    public Contract? SelectedContract
    {
        get => _selectedContract;
        set => this.RaiseAndSetIfChanged(ref _selectedContract, value);
    }

    public Contract? BeforeEditingContract
    {
        get => _beforeEditingContract;
        set => this.RaiseAndSetIfChanged(ref _beforeEditingContract, value);
    }

    public ContractsViewModel()
    {
        LogHost.Default.Debug("ContractsViewModel начальная инициализация");

        _dbContext = Locator.Current.GetService<DataBaseContext>()!;
        _appSession = Locator.Current.GetService<AppSession>()!;
        
        UpdateWageLogs();


        IsEditing = false;
        IsAdding = false;
        SelectedContract = new();
        Task.Run(LoadUserContracts);

        CanDeleteSelectedContract = this.WhenAnyValue(
            x => x.UserContracts,
            x => x.SelectedContract,
            x => x.IsEditing,
            (contracts, contract, adding) =>
            {
                if (contract is null) return false;
                if (adding) return false;
                return contracts.Any(c => c.Id == contract.Id);
            });

        CancelEditContractCommand = ReactiveCommand.Create(
            CancelEditingContract,
            this.WhenAnyValue(
                x => x.IsEditing,
                x => x.UserContracts,
                x => x.SelectedContract,
                x => x.IsAdding,
                (editing, courses, course, adding) =>
                {
                    if (adding) return true;
                    if (!editing || course is null) return false;
                    return courses.Any(c => c.Id == course.Id);
                }));

        DeleteContractCommand = ReactiveCommand.Create(DeleteContract, CanDeleteSelectedContract);
        EditContractCommand = ReactiveCommand.Create(
            EditContract,
            this.WhenAnyValue(
                x => x.IsEditing,
                x => x.SelectedContract,
                (editing, course) => !editing && course is not null));

        SaveContractCommand = ReactiveCommand.Create(SaveContract, CanSaveContract());

        AddContractCommand = ReactiveCommand.Create(
            AddContract,
            this.WhenAnyValue(x => x.IsEditing, x => !x));

        CurrentState = Localization.Localization.ViewText;

        _dbContext.WhenErrorMessageChanged
            .BindTo(this, vm => vm.ErrorMessage);

        LogHost.Default.Debug("CoursesViewModel инициализация завершена");
    }

    private async Task LoadUserContracts()
    {
        await foreach (var contract in _dbContext.GetUserContractsAsyncEnumerable(_appSession.CurrentAccount!)!)
        {
            UserContracts.Add(contract);
        }

        if (UserContracts.Count == 0)
        {
            ErrorMessage =
                $"Для пользователя {_appSession.CurrentAccount!.Login} контракты не найдены. Добавьте новый контракт.";
        }
        else
        {
            ErrorMessage =
                $"Для пользователя {_appSession.CurrentAccount!.Login} контракты найдены.";
        }
    }

    private void CancelEditingContract()
    {
        LogHost.Default.Debug("Отмена редактирования контракта");
        try
        {
            if (IsAdding)
            {
                SelectedContract = null;
                IsAdding = false;
                LogHost.Default.Info("Добавление контракта отменено");
            }
            else if (_beforeEditingContract is not null && SelectedContract is not null)
            {
                var index = UserContracts.IndexOf(SelectedContract);
                if (index >= 0)
                {
                    UserContracts[index] = new Contract(_beforeEditingContract);
                    LogHost.Default.Info($"Изменения для контракта за {_beforeEditingContract.BeginDate} отменены");
                }
            }

            _beforeEditingContract = null;
            IsEditing = false;
            CurrentState = Localization.Localization.ViewText;
        }
        catch (Exception ex)
        {
            LogHost.Default.Error(ex, "Ошибка при отмене редактирования контракта");
            ErrorMessage = $"Ошибка отмены: {ex.Message}";
        }
    }

    private void SaveContract()
    {
        LogHost.Default.Debug("Сохранение изменений контракта");
        try
        {
            if (IsAdding)
            {
                var existing = UserContracts.FirstOrDefault(x =>
                    x.BeginDate.Date == SelectedContract?.BeginDate.Date);

                if (existing is null)
                {
                    _dbContext.AddContract(SelectedContract!);
                    UserContracts.Insert(0, new Contract(SelectedContract!));
                    ErrorMessage = $"Добавлен контракт за {SelectedContract!.BeginDate:d}";
                    LogHost.Default.Info(ErrorMessage);
                }
                else
                {
                    ErrorMessage = "Контракт с такой датой начала уже существует.";
                    LogHost.Default.Warn($"Попытка добавить дубликат контракта за {SelectedContract?.BeginDate}");
                }

                IsAdding = false;
            }
            else
            {
                if (SelectedContract.BeginDate >= SelectedContract.EndDate)
                {
                    ErrorMessage = "Конец контракта не может быть позже начала!";
                    return;
                }

                if (String.IsNullOrWhiteSpace(SelectedContract.VesselName))
                {
                    ErrorMessage = "Название судна не может быть пустым!";
                    return;
                }

                _dbContext.UpdateContract(BeforeEditingContract!, SelectedContract!);

                if (UserContracts.Contains(SelectedContract!))
                {
                    var index = UserContracts.IndexOf(SelectedContract!);
                    UserContracts[index] = new Contract(SelectedContract);

                    ErrorMessage = $"контракт за {SelectedContract?.BeginDate:d} сохранён";
                    LogHost.Default.Info($"контракт за {SelectedContract?.BeginDate} обновлён");
                }
                else
                {
                    LogHost.Default.Error("Ошибка при сохранении контракта");
                    ErrorMessage = "Ошибка Сохранения курса";
                }
            }

            BeforeEditingContract = null;
            SelectedContract = null;
            CurrentState = Localization.Localization.ViewText;
            IsEditing = false;
        }
        catch (Exception ex)
        {
            LogHost.Default.Error(ex, "Ошибка при сохранении контракта");
            ErrorMessage = $"Ошибка сохранения: {ex.Message}";
        }
    }

    private IObservable<bool> CanSaveContract()
    {
        LogHost.Default.Debug("Оценка возможности сохранения контракта");
        return this.WhenAnyValue(
            x => x.IsEditing,
            x => x.IsAdding,
            ( isEditing, isAdding) =>
            {
                if (isAdding) return true;
                return isEditing;
            });
    }

    private void DeleteContract()
    {
        LogHost.Default.Debug("Попытка удаления контракта");
        try
        {
            if (SelectedContract is null)
            {
                LogHost.Default.Warn("Попытка удаления: SelectedCourse равен null");
                return;
            }
            
            // if ()
            // {
            //     ErrorMessage = " WageLogs имеют записи c данным контрактом, сначала удалите их.";
            //     LogHost.Default.Warn("Попытка удаления: WageLogs имеют записи");
            //     return;
            // }
            

            var searchedCourse = UserContracts.FirstOrDefault(x => x.BeginDate == SelectedContract.BeginDate);
            if (searchedCourse is null)
            {
                LogHost.Default.Warn("контракт для удаления не найден в коллекции");
                return;
            }

            UserContracts.Remove(searchedCourse);
            _dbContext.DeleteContract(searchedCourse);
            SelectedContract = null;
            IsEditing = false;

            ErrorMessage = $"контракт за {searchedCourse.BeginDate:dd.MM.yyyy} удалён";
            LogHost.Default.Info($"контракт за {searchedCourse.BeginDate} успешно удалён");
        }
        catch (Exception ex)
        {
            LogHost.Default.Error(ex, "Ошибка при удалении контракта");
            ErrorMessage = $"Ошибка удаления: {ex.Message}";
        }
    }

    private void EditContract()
    {
        LogHost.Default.Debug("Редактирование контракта начато");
        try
        {
            if (SelectedContract is null)
            {
                LogHost.Default.Warn("Попытка редактирования: SelectedCourse равен null");
                return;
            }

            IsEditing = true;
            BeforeEditingContract = new Contract(SelectedContract);
            CurrentState = Localization.Localization.EditingText;

            LogHost.Default.Info($"Начато редактирование контракта за {SelectedContract.BeginDate}");
        }
        catch (Exception ex)
        {
            LogHost.Default.Error(ex, "Ошибка при старте редактирования контракта");
            ErrorMessage = $"Ошибка редактирования: {ex.Message}";
        }
    }

    private void AddContract()
    {
        LogHost.Default.Debug("Добавление нового контракта начато");
        try
        {
            SelectedContract = new Contract()
            {
                BeginDate = DateTime.Now.Date,
                EndDate = DateTime.Now.Date + new TimeSpan(120, 0, 0, 0),
                Account = _appSession.CurrentAccount,
                VesselName = "Судно",
                ContractDescription = "Описание",
                Position = _appSession.CurrentAccount.Position
            };
            IsAdding = true;
            IsEditing = true;
            CurrentState = Localization.Localization.AddingText;

            LogHost.Default.Info($"Новый контракта создан с датой {SelectedContract.BeginDate}");
        }
        catch (Exception ex)
        {
            LogHost.Default.Error(ex, "Ошибка при создании нового контракта");
            ErrorMessage = $"Ошибка добавления: {ex.Message}";
        }
    }

    public void UpdateWageLogs()
    {
        WageLogs = _dbContext.WageLogs
            .Include(wlall => wlall.Contract)
            .Select(wl => wl.Contract)
            .Where(x => UserContracts.Any(uc => uc.Id == x.Id))
            .AsEnumerable();
    }
}