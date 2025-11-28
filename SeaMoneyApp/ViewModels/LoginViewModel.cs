using System;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Windows.Input;
using System.Reactive.Linq;
using System.Reactive;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SeaMoneyApp.Services.Authorization;
using Splat;

namespace SeaMoneyApp.ViewModels;

[DataContract]
public partial class LoginViewModel : ViewModelBase, IRoutableViewModel
{
    public IScreen HostScreen { get; }
    public string UrlPathSegment => "/login";
   
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


    public LoginViewModel(IScreen? screen = null)
    {
        var authService = Locator.Current.GetService<IAuthorizationService>() 
                           ?? throw new InvalidOperationException("IAuthorizationService not registered");

        HostScreen = screen ?? Locator.Current.GetService<IScreen>() 
            ?? throw new InvalidOperationException("IScreen not registered");

        LoginCommand = ReactiveCommand.Create(() =>
        {
            if (authService.Login(Username!, Password!))
            {
                authService.FlushErrorMessage();
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

        ToRegistrationCommand = ReactiveCommand.Create(() =>
        {
            HostScreen.Router.Navigate.Execute(new RegistrationViewModel());
        });
        

        // Подписываемся на изменения ошибки
        authService.WhenErrorMessageChanged
            .BindTo(this, vm => vm.ErrorMessage);

        //  сбрасывать ошибку при изменении полей
        //this.WhenAnyValue(x => x.Username, x => x.Password)
        //     .Subscribe(_ => ErrorMessage = string.Empty);
        
        // this.WhenAnyValue(x => x.Username, x => x.Password)
        //     .Subscribe(x =>
        //     {
        //         var can = !string.IsNullOrWhiteSpace(x.Item1) && !string.IsNullOrWhiteSpace(x.Item2);
        //         //LogHost.Default.Info($"Username: '{x.Item1}', Password: '{x.Item2}' -> CanLogin: {can}");
        //         //LogHost.Default.Info($"CanLogin updated: {can}");
        //     });
    }
}