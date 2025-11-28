using System;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using SeaMoneyApp.Services.Authorization;
using Splat;

namespace SeaMoneyApp.ViewModels;

public class RegistrationViewModel: ViewModelBase, IRoutableViewModel
{
    public IScreen HostScreen { get; }
    public string UrlPathSegment => "/login";
    //[Reactive] public string? Password { get; set; }
    private string? _password = string.Empty;
    public string? Password
    {
        get => _password;
        set
        {
            this.RaiseAndSetIfChanged(ref _password, value);
            //LogHost.Default.Info($"Password changed to: {value}");
        }
    }
    //[Reactive] public string? ErrorMessage { get; set; }
    private string? _errorMessage = string.Empty;
    public string? ErrorMessage
    {
        get => _errorMessage;
        set
        {
            this.RaiseAndSetIfChanged(ref _errorMessage, value);
            //LogHost.Default.Info($"ErrorMessage changed to: {value}");
        }
    }
    //[DataMember] [Reactive] public string? Username { get; set; }
    private string? _username = string.Empty;
    public string? Username
    {
        get => _username;
        set
        {
            this.RaiseAndSetIfChanged(ref _username, value);
            //LogHost.Default.Info($"Username changed to: {value}");
        }
    }
    public ReactiveCommand<Unit, Unit> LoginCommand { get; private set; } 

    public RegistrationViewModel(IScreen? screen = null)
    {
        var authService = Locator.Current.GetService<IAuthorizationService>() 
                           ?? throw new InvalidOperationException("IAuthorizationService not registered");

        HostScreen = screen ?? Locator.Current.GetService<IScreen>() 
            ?? throw new InvalidOperationException("IScreen not registered");

        LoginCommand = ReactiveCommand.Create(() =>
        {
            if (authService.Login(Username!, Password!))
            {
               HostScreen.Router.Navigate.Execute(new SearchViewModel());
            }
            else
            {
                return;
            }
        },
            this
            .WhenAnyValue(x => x.Username, x => x.Password)
            .Select(x => !string.IsNullOrWhiteSpace(x.Item1) && !string.IsNullOrWhiteSpace(x.Item2)));

        // Подписываемся на изменения ошибки
        authService.WhenErrorMessageChanged
            .BindTo(this, vm => vm.ErrorMessage);

        //  сбрасывать ошибку при изменении полей
        //this.WhenAnyValue(x => x.Username, x => x.Password)
        //     .Subscribe(_ => ErrorMessage = string.Empty);
        
        this.WhenAnyValue(x => x.Username, x => x.Password)
            .Subscribe(x =>
            {
                var can = !string.IsNullOrWhiteSpace(x.Item1) && !string.IsNullOrWhiteSpace(x.Item2);
                LogHost.Default.Info($"Username: '{x.Item1}', Password: '{x.Item2}' -> CanLogin: {can}");
                LogHost.Default.Info($"CanLogin updated: {can}");
            });
    }
}