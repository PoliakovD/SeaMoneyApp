using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Reactive.Linq;
using System.Reactive;
using System.Threading;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SeaMoneyApp.DataAccess;
using SeaMoneyApp.DataAccess.Models;
using SeaMoneyApp.Extensions;
using SeaMoneyApp.Models;
using SeaMoneyApp.Services.Authorization;
using SeaMoneyApp.Views;
using Splat;

namespace SeaMoneyApp.ViewModels;

public class AccountViewModel : RoutableViewModel
{
    private const int SaveTimeout = 2000;
    public AppSession AppSession { get; }
    private Account? _beforeEditingAccount;
    private Account _currentAccount;
    private Position? _viewedPosition;
    private Position? _beforeEditingPosition;
    private bool _isAdminPanelEnabled;
    
    public ReactiveCommand<Unit, Unit>? LogOutCommand { get; private set; }
    public ReactiveCommand<Unit, Unit>? ToAdminPanelCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> EditAccountCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelEditAccountCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveAccountCommand { get; }
    private bool _isEditing;
    private string _currentState;
    private string? _errorMessage;
    
    public ObservableCollection<Position> Positions { get; set; } 
    public string CurrentState
    {
        get => _currentState;
        set => this.RaiseAndSetIfChanged(ref _currentState, value);
    }

    public bool IsEditing
    {
        get => _isEditing;
        private set => this.RaiseAndSetIfChanged(ref _isEditing, value);
    }
    
    public bool IsAdminPanelEnabled
    {
        get => _isAdminPanelEnabled;
        private set => this.RaiseAndSetIfChanged(ref _isAdminPanelEnabled, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }


    public Account? CurrentAccount
    {
        get => _currentAccount;
        set => this.RaiseAndSetIfChanged(ref _currentAccount, value);
    }
    public Position? ViewedPosition
    {
        get => _viewedPosition;
        set => this.RaiseAndSetIfChanged(ref _viewedPosition, value);
    }
    public AccountViewModel()
    {
        var authService = Locator.Current.GetService<IAuthorizationService>()
                          ?? throw new InvalidOperationException("IAuthorizationService not registered");
        AppSession = Locator.Current.GetService<AppSession>()!;
        
        var appAcc = AppSession.CurrentAccount;
        
        CurrentAccount = new Account()
        {
            Id = appAcc!.Id,
            Login = appAcc.Login,
            ToursInRank = appAcc.ToursInRank,
            Password = appAcc.Password,
            Position = new Position()
            {
                Id = appAcc.Position!.Id,
                Name = appAcc.Position.Name
            }
        };

        InitPositions();
        
        IsEditing = false;
        CurrentState = Localization.Localization.ViewText;
        
        LogOutCommand = ReactiveCommand.Create(() =>
            {
                Locator.Current.GetService<IAuthorizationService>().Logout();
                AppSession.ClearSavedSession();
                GoToLoginView();
            },
            this.WhenAnyValue(x => x.AppSession.CurrentAccount)
                .Any(x => x != null));
        
        ToAdminPanelCommand = ReactiveCommand.Create(GoToAdminPaneliVew,
            this.WhenAnyValue(x => x.AppSession, (a)=>a.IsAdmin));
       
        authService.WhenErrorMessageChanged
            .BindTo(this, vm => vm.ErrorMessage);

        EditAccountCommand = ReactiveCommand.Create(EditAccount,
            this.WhenAnyValue(x => x.IsEditing,
                isediting => !isediting));

        SaveAccountCommand = ReactiveCommand.CreateFromTask(SaveAccount,
            this.WhenAnyValue(x => x.IsEditing));

        CancelEditAccountCommand = ReactiveCommand.Create(CancelEditAccount,
            this.WhenAnyValue(x => x.IsEditing));
    }

    private void GoToLoginView()
    {
        // создаем новую вьюху Overall и регестрируем ее как текущую для навигации назад
        var main = Locator.Current.GetService<MainViewModel>();

        Locator.CurrentMutable.RegisterConstant<IScreenBackCommand>(main);

        // очищаем кеш предыдущего рутера
        HostScreen.Router.ClearCache();

        // Переходим к новой вьюхе
        HostScreen!.Router.NavigateAndCache<LoginViewModel>();
    }
    
    private void GoToAdminPaneliVew()
    {
        var check = Locator.Current.GetService<IScreenBackCommand>() is OverallViewModel;
        if (check)
        {
            var backRouter = Locator.Current.GetService<IScreenBackCommand>().Router;
        
            // Переходим к новой вьюхе
            backRouter.NavigateAndCache<AdminPanelMainViewModel>();
        }
        
    }

    private void InitPositions()
    {
        Positions = new ObservableCollection<Position>();

        var positions = Locator.Current.GetService<DataBaseContext>()!.Positions.ToList();

        foreach (var position in positions)
        {
            Positions!.Add(position);
        }
        ViewedPosition = Positions.First(p=>p.Id == CurrentAccount.Position.Id);
    }

    private void EditAccount()
    {
        LogHost.Default.Debug("Редактирование аккаунта начато");
        try
        {
            _beforeEditingPosition = new Position() { Id = ViewedPosition!.Id, Name = ViewedPosition.Name };
            _beforeEditingAccount = new Account()
            {
                Id = CurrentAccount!.Id,
                ToursInRank = CurrentAccount.ToursInRank,
                Password = CurrentAccount.Password,
                Login = CurrentAccount.Login,
                Position = new Position()
                {
                    Id = _beforeEditingPosition!.Id,
                    Name = _beforeEditingPosition.Name
                }
            }; 
            
            if (_beforeEditingAccount?.Login is null)
            {
                LogHost.Default.Warn("Попытка редактирования: beforeEditingAccount  равен null");
                return;
            }

            IsEditing = true;
            CurrentState = Localization.Localization.EditingText;
            LogHost.Default.Info($"Начато редактирование аккаунта {_beforeEditingAccount.Login}");
        }
        catch (Exception ex)
        {
            LogHost.Default.Error(ex, "Ошибка при старте редактирования аккаунта");
            ErrorMessage = $"Ошибка редактирования: {ex.Message}";
        }
    }

    private void CancelEditAccount()
    {
        LogHost.Default.Debug("Отмена редактирования аккаунта начато");
        var appAcc = AppSession.CurrentAccount;
        CurrentAccount = new Account()
        {
            Id = appAcc!.Id,
            Login = appAcc.Login,
            ToursInRank = appAcc.ToursInRank,
            Password = appAcc.Password,
            Position = new Position()
            {
                Id = appAcc.Position!.Id,
                Name = appAcc.Position.Name
            }
        };
        IsEditing = false;
        CurrentState = Localization.Localization.ViewText;
        
        ViewedPosition = Positions.First(p=>p.Id == CurrentAccount.Position.Id);
        
        LogHost.Default.Debug("Закончили редактирование аккаунта начато");
    }

    private async Task SaveAccount()
    {
        LogHost.Default.Debug("Сохранение изменений Аккаунта");
        try
        {
            var cts = new CancellationTokenSource(SaveTimeout);
            var token = cts.Token;
            var authService = Locator.Current.GetService<IAuthorizationService>()
                              ?? throw new InvalidOperationException("IAuthorizationService not registered");
            CurrentAccount.Position = ViewedPosition;

            var result = await authService.UpdateAccountAsync(_beforeEditingAccount!, CurrentAccount!, token);

            if (result)
            {
                CurrentState = Localization.Localization.ViewText;
                IsEditing = false;
                await AppSession.SaveSessionAsync();
            }
            if (_viewedPosition is null)
            {
                _viewedPosition = Positions.First(p=>p.Id == _beforeEditingPosition!.Id);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            LogHost.Default.Error("Ошибка при сохранении курса");
            LogHost.Default.Error(ex.Message);
            //ErrorMessage = "Ошибка Сохранения курса";
        }
    }
}