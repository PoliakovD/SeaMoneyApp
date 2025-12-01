using System;
using ReactiveUI;
using SeaMoneyApp.DataAccess.Models;

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
}