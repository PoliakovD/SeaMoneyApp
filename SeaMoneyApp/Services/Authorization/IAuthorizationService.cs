using System;
using System.Threading.Tasks;
using SeaMoneyApp.DataAccess.Models;

namespace SeaMoneyApp.Services.Authorization;


public interface IAuthorizationService
{
    bool IsLoggedIn { get; }
    IObservable<DateTime?> LastLoginTimeChanged { get; }
    IObservable<bool> WhenLoggedInChanged { get; }
    IObservable<Account?> WhenAccountInChanged { get; }
    IObservable<string?> WhenErrorMessageChanged { get; }
    bool Login(string username, string password);
    bool Register(string? login, string? password, Position? position, short? toursInRank);
    void Logout();
    void FlushErrorMessage();
}