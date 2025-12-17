using System;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Windows.Input;
using System.Reactive.Linq;
using System.Reactive;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SeaMoneyApp.Extensions;
using SeaMoneyApp.Models;
using SeaMoneyApp.Services.Authorization;
using Splat;

namespace SeaMoneyApp.ViewModels;

[DataContract]
public partial class LoginViewModel : RoutableViewModel
{
    private string? _password = string.Empty;
    public ReactiveCommand<Unit, Unit>? LoginCommand { get; private set; }
    public ReactiveCommand<Unit, Unit>? ToRegistrationCommand { get; private set; }

    public string? Password
    {
        get => _password;
        set { this.RaiseAndSetIfChanged(ref _password, value); }
    }

    private string? _errorMessage = string.Empty;

    public string? ErrorMessage
    {
        get => _errorMessage;
        set { this.RaiseAndSetIfChanged(ref _errorMessage, value); }
    }

    private string? _username = string.Empty;

    public string? Username
    {
        get => _username;
        set { this.RaiseAndSetIfChanged(ref _username, value); }
    }

    private bool _rememberMe = false;

    public bool RememberMe
    {
        get => _rememberMe;
        set { this.RaiseAndSetIfChanged(ref _rememberMe, value); }
    }


    public LoginViewModel()
    {
        var authService = Locator.Current.GetService<IAuthorizationService>()
                          ?? throw new InvalidOperationException("IAuthorizationService not registered");


        LoginCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if (authService.Login(Username, Password, RememberMe))
                {
                    // очищаем ErrorMessage
                    authService.FlushErrorMessage();
                    GoToOverallView();
                    await Locator.Current.GetService<AppSession>()!.SaveSessionAsync();
                }
            },
            this
                .WhenAnyValue(x => x.Username, x => x.Password)
                .Select(x => !string.IsNullOrWhiteSpace(x.Item1) && !string.IsNullOrWhiteSpace(x.Item2)));

        ToRegistrationCommand = ReactiveCommand.Create(() =>
        {
            HostScreen!.Router.NavigateAndCache<RegistrationViewModel>();
        });


        // Подписываемся на изменения ошибки
        authService.WhenErrorMessageChanged
            .BindTo(this, vm => vm.ErrorMessage);
        
        CheckRestoredAppSession();
    }

    private void GoToOverallView()
    {
        // создаем новую вьюху Overall и регестрируем ее как текущую для навигации назад
        var overall = new OverallViewModel();
        Locator.CurrentMutable.RegisterConstant<IScreenBackCommand>(overall);

        // очищаем кеш предыдущего рутера
        HostScreen.Router.ClearCache();

        // Переходим к новой вьюхе
        HostScreen!.Router.NavigateAndCache<OverallViewModel>(overall);
    }

    private void CheckRestoredAppSession()
    {
        LogHost.Default.Debug("CheckRestoredAppSession started");
        var appSession = Locator.Current.GetService<AppSession>();
        LogHost.Default.Debug($"CheckRestoredAppSession appSession is null? = {appSession == null}");
        LogHost.Default.Warn($"App session {appSession}");
        if (appSession.CurrentAccount != null)
        {
            GoToOverallView();
        }
    }
}