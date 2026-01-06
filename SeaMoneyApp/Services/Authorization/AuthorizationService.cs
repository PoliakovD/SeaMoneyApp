using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using SeaMoneyApp.DataAccess;
using SeaMoneyApp.DataAccess.Models;
using SeaMoneyApp.Models;
using SeaMoneyApp.Localization;
using Splat;


namespace SeaMoneyApp.Services.Authorization;

/// <summary>
/// Реализация сервиса авторизации.
/// </summary>
public class AuthorizationService : IAuthorizationService, IDisposable
{
    private readonly DataBaseContext _dbContext = Locator.Current.GetService<DataBaseContext>()!;

    private readonly BehaviorSubject<bool> _isLoggedInSubject = new(false);
    private readonly BehaviorSubject<Account?> _loggedInAccount = new(null);
    private readonly BehaviorSubject<string?> _errorMessageSubject = new(null);
    private readonly BehaviorSubject<DateTime?> _lastLoginTime = new(null);
    private readonly BehaviorSubject<bool> _rememberMeSubject = new(false);

    public IObservable<DateTime?> LastLoginTimeChanged => _lastLoginTime.AsObservable();

    public IObservable<bool> WhenLoggedInChanged => _isLoggedInSubject.AsObservable();

    public IObservable<bool> WhenRememberMeChanged => _rememberMeSubject.AsObservable();

    public IObservable<Account?> WhenAccountInChanged => _loggedInAccount.AsObservable();

    public IObservable<string?> WhenErrorMessageChanged => _errorMessageSubject.AsObservable();
    

    public bool Login(string? username, string? password, bool rememberMe = false)
    {
        _errorMessageSubject.OnNext(null); // Сброс ошибки
        // Проверка ввода
        var validationResult = ValidateCredentials(username, password);
        if (!validationResult.IsValid)
        {
            var errorMsg = validationResult.ErrorMessage;
            LogHost.Default.Warn($"Login failed: {errorMsg}");
            _errorMessageSubject.OnNext(errorMsg);
            return false;
        }

        // TODO use Hash(password)
        var account = _dbContext.Accounts
            .Include(a => a.Position)
            .FirstOrDefault(u => u.Login == username);
        if (account == null)
        {
            var errorMsg = "User " + username + " not found";
            LogHost.Default.Info(errorMsg);
            _errorMessageSubject.OnNext(errorMsg);
            return false;
        }

        if (account.Password == password)
        {
            // Уведомляем подписчиков
            _isLoggedInSubject.OnNext(true);
            _loggedInAccount.OnNext(account);
            _errorMessageSubject.OnNext(null);
            _lastLoginTime.OnNext(DateTime.Now);
            _rememberMeSubject.OnNext(rememberMe);


            LogHost.Default.Info("User logged in: " + username);
            return true;
        }

        var errorMsgLast = "Wrong Password!";
        LogHost.Default.Info(errorMsgLast);
        _errorMessageSubject.OnNext(errorMsgLast);
        return false;
    }

    public bool Register(string? login, string? password, Position? position, short? toursInRank)
    {
        _errorMessageSubject.OnNext(null); // Сброс ошибки
        // Проверка ввода
        var validationResult = ValidateCredentials(login, password);
        if (!validationResult.IsValid)
        {
            var errorMsg = validationResult.ErrorMessage;
            LogHost.Default.Warn($"Registration failed: {errorMsg}");
            _errorMessageSubject.OnNext(errorMsg);
            return false;
        }

        if (position is null)
        {
            var errorMsg = "Position not found, Please select a position from list";
            LogHost.Default.Warn($"Registration failed: {errorMsg}");
            _errorMessageSubject.OnNext(errorMsg);
            return false;
        }

        if (toursInRank is null)
        {
            var errorMsg = "Tours Cannot be null, Please select a tour";
            LogHost.Default.Warn($"Registration failed: {errorMsg}");
            _errorMessageSubject.OnNext(errorMsg);
            return false;
        }

        if (toursInRank < 0)
        {
            var errorMsg = "Tours Cannot be negative";
            LogHost.Default.Warn($"Registration failed: {errorMsg}");
            _errorMessageSubject.OnNext(errorMsg);
            return false;
        }


        if (_dbContext.Accounts.FirstOrDefault(u => u.Login == login) is null)
        {
            var account = new Account()
            {
                Login = login,
                Password = password,
                Position = position,
                ToursInRank = (short)toursInRank
            };

            _dbContext.Accounts.Add(account);
            // Уведомляем подписчиков
            _dbContext.SaveChanges();
            _isLoggedInSubject.OnNext(true);
            _errorMessageSubject.OnNext(null);
            _loggedInAccount.OnNext(account);
            _lastLoginTime.OnNext(DateTime.Now);

            LogHost.Default.Debug("User registred and logged in: " + login);
            return true;
        }
        else
        {
            var errorMsg = "Cannot register user already exist: " + login;
            LogHost.Default.Debug(errorMsg);
            _errorMessageSubject.OnNext(errorMsg);
            return false;
        }
    }

    public void Logout()
    {
        if (!_isLoggedInSubject.Value) return;
        
        _isLoggedInSubject.OnNext(false);
        _loggedInAccount.OnNext(null);
        _errorMessageSubject.OnNext(" ");
        _lastLoginTime.OnNext(null);

        LogHost.Default.Info("User logged out");
    }

    private ValidationResult ValidateCredentials(string username, string password)
    {
        // Проверка логина
        if (string.IsNullOrWhiteSpace(username))
            return new ValidationResult(false, "Логин не может быть пустым");

        if (username.Length < 3)
            return new ValidationResult(false, "Логин должен быть не менее 3 символов");

        if (username.Length > 50)
            return new ValidationResult(false, "Логин не должен превышать 50 символов");

        if (!IsValidUsername(username))
            return new ValidationResult(false,
                "Логин может содержать только буквы, цифры, точки, тире и подчёркивания");

        // Проверка пароля
        if (string.IsNullOrWhiteSpace(password))
            return new ValidationResult(false, "Пароль не может быть пустым");

        if (password.Length < 5)
            return new ValidationResult(false, "Пароль должен быть не менее 5 символов");

        if (password.Length > 32)
            return new ValidationResult(false, "Пароль слишком длинный > 32 символов");


        return new ValidationResult(true, "OK");
    }

    private bool IsValidUsername(string username)
    {
        foreach (char c in username)
        {
            if (!char.IsLetterOrDigit(c) && c != '.' && c != '-' && c != '_')
                return false;
        }

        return true;
    }

    public void FlushErrorMessage() => _errorMessageSubject.OnNext(null);

    public void Dispose()
    {
        _isLoggedInSubject?.Dispose();
        _loggedInAccount?.Dispose();
        _errorMessageSubject?.Dispose();
        _lastLoginTime?.Dispose();
    }

    public async Task<bool> UpdateAccountAsync(Account oldAccount, Account newAccount, CancellationToken token)
    {
        try
        {
            token.ThrowIfCancellationRequested();
            var validation = ValidateCredentials(newAccount.Login, newAccount.Password);
            if (!validation.IsValid)
            {
                _errorMessageSubject.OnNext(validation.ErrorMessage);
                return false;
            }
            var secondValidation = ValidateAccountForUpdate(oldAccount, newAccount);
            if (!secondValidation.IsValid)
            {
                _errorMessageSubject.OnNext(secondValidation.ErrorMessage);
                return false;
            }
            _errorMessageSubject.OnNext(Localization.Localization.AccountSuccessfulyChangedText);
            await _dbContext.UpdateAccountAsync(oldAccount,newAccount, token);
            _loggedInAccount.OnNext(newAccount);
            return true;
        }
        catch (OperationCanceledException oc)
        {
            _errorMessageSubject.OnNext(Localization.Localization.SaveTimeOutText);
            LogHost.Default.Error(oc.Message);
        }
        catch (Exception e)
        {
            _errorMessageSubject.OnNext(e.Message);
            LogHost.Default.Error(e.Message);
        }

        return false;
    }

    private ValidationResult ValidateAccountForUpdate(Account oldAccount, Account newAccount)
    {
        if (oldAccount.Login == newAccount.Login &&
            oldAccount.Password == newAccount.Password &&
            oldAccount.ToursInRank == newAccount.ToursInRank &&
            oldAccount.Position!.Name == newAccount.Position!.Name)
            return new ValidationResult(false, Localization.Localization.AccountUnchangedText);
        if (oldAccount.Login != newAccount.Login)
        {
            var checkLoginFree = CheckLoginFree(newAccount.Login);
            if (!checkLoginFree) return new ValidationResult(false, Localization.Localization.loginOccupiedText);
        }
        if (newAccount.ToursInRank<0||newAccount.ToursInRank>8) 
            return new ValidationResult(false, Localization.Localization.InvalidToursInRankText);
        if (newAccount.Position==null)
            return new ValidationResult(false, Localization.Localization.InvalidPositionText);
        
        return new ValidationResult(true, "Ok");
    }
    private bool CheckLoginFree(string login)
    => _dbContext.Accounts.FirstOrDefault(u => u.Login == login) is null;
}

internal record ValidationResult(bool IsValid, string ErrorMessage);