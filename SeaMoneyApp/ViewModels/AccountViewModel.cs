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

public class AccountViewModel: RoutableViewModel
{
    public AppSession AppSession { get; }
    public ReactiveCommand<Unit, Unit>? LogOutCommand { get; private set; }
    public AccountViewModel()
    {
        AppSession = Locator.Current.GetService<AppSession>()!;
        LogOutCommand = ReactiveCommand.Create(() =>
        {
            Locator.Current.GetService<IAuthorizationService>().Logout();
            AppSession.ClearSavedSession();
            GoToLoginView();
        }, 
            this.WhenAnyValue(x => x.AppSession.CurrentAccount).Any(x => x != null));
    }
    private void GoToLoginView()
    {
        // создаем новую вьюху Overall и регестрируем ее как текущую для навигации назад
        var main =  Locator.Current.GetService<MainViewModel>();
        
        Locator.CurrentMutable.RegisterConstant<IScreenBackCommand>(main);

        // очищаем кеш предыдущего рутера
        HostScreen.Router.ClearCache();

        // Переходим к новой вьюхе
        HostScreen!.Router.NavigateAndCache<LoginViewModel>();
    }
}