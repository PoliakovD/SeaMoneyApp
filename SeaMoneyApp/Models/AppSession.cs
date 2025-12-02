using System;
using ReactiveUI;
using SeaMoneyApp.DataAccess.Models;
using SeaMoneyApp.Services.Authorization;
using Splat;

namespace SeaMoneyApp.Models;


public class AppSession : ReactiveObject
{
    private Account? _currentAccount;
    private DateTime? _lastLoginTime;

    public Account? CurrentAccount
    {
        get => _currentAccount;
        set => this.RaiseAndSetIfChanged(ref _currentAccount, value);
    }

    public DateTime? LastLoginTime
    {
        get => _lastLoginTime;
        set => this.RaiseAndSetIfChanged(ref _lastLoginTime, value);
    }

    public bool IsLoggedIn => CurrentAccount is not null;

    public AppSession()
    {
        var authService = Locator.Current.GetService<IAuthorizationService>();
        // Подписываемся на изменения ошибки
        authService!.WhenAccountInChanged
            .BindTo(this, vm => vm.CurrentAccount);
        authService!.LastLoginTimeChanged
            .BindTo(this, vm => vm.LastLoginTime);
    }
}