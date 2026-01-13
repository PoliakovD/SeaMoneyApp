using System;
using System.Threading;
using System.Threading.Tasks;
using SeaMoneyApp.DataAccess.Models;

namespace SeaMoneyApp.Services.Authorization;


public interface IAuthorizationService
{
    IObservable<DateTime?> LastLoginTimeChanged { get; }
    IObservable<bool> WhenLoggedInChanged { get; }
    IObservable<Account?> WhenAccountInChanged { get; }
    IObservable<string?> WhenErrorMessageChanged { get; }
    IObservable<bool> WhenRememberMeChanged { get; }
    bool Login(string? username, string? password, bool rememberMe);
    void AutoLogin(string? username);
    bool Register(string? login, string? password, Position? position, short? toursInRank);
    void Logout();
    void FlushErrorMessage();
    Task<bool> UpdateAccountAsync(Account oldAccount, Account newAccount, CancellationToken token);

}