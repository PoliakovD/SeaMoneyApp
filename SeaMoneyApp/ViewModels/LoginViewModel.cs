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
using SeaMoneyApp.Services.Authorization;
using Splat;

namespace SeaMoneyApp.ViewModels;

[DataContract]
public partial class LoginViewModel : RoutableViewModel
{
   
    private string? _password = string.Empty;
    public ReactiveCommand<Unit, Unit> LoginCommand { get; private set; } 
    public ReactiveCommand<Unit, Unit>ToRegistrationCommand { get; private set; }
    public string? Password
    {
        get => _password;
        set
        {
            this.RaiseAndSetIfChanged(ref _password, value);
        }
    }
    private string? _errorMessage = string.Empty;
    public string? ErrorMessage
    {
        get => _errorMessage;
        set
        {
            this.RaiseAndSetIfChanged(ref _errorMessage, value);
           
        }
    }
   
    private string? _username = string.Empty;
    public string? Username
    {
        get => _username;
        set
        {
            this.RaiseAndSetIfChanged(ref _username, value);
           
        }
    }


    public LoginViewModel()
    {
        var authService = Locator.Current.GetService<IAuthorizationService>() 
                           ?? throw new InvalidOperationException("IAuthorizationService not registered");
        

        LoginCommand = ReactiveCommand.Create(() =>
        {
            if (authService.Login(Username!, Password!))
            {
                authService.FlushErrorMessage();
                HostScreen.Router.NavigateAndCache<SearchViewModel>();
            }
        },
            this
            .WhenAnyValue(x => x.Username, x => x.Password)
            .Select(x => !string.IsNullOrWhiteSpace(x.Item1) && !string.IsNullOrWhiteSpace(x.Item2)));

        ToRegistrationCommand = ReactiveCommand.Create(() =>
        {
            HostScreen.Router.NavigateAndCache<RegistrationViewModel>();
        });
        

        // Подписываемся на изменения ошибки
        authService.WhenErrorMessageChanged
            .BindTo(this, vm => vm.ErrorMessage);

       
    }
}