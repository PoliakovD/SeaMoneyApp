using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SeaMoneyApp.DataAccess;
using SeaMoneyApp.DataAccess.Models;
using Splat;


namespace SeaMoneyApp.Services.Authorization;

/// <summary>
/// Реализация сервиса авторизации.
/// </summary>
public class AuthorizationService : IAuthorizationService
{
    private readonly DataBaseContext _dbContext = Locator.Current.GetService<DataBaseContext>()!;
    private bool _isLoggedIn;
    private readonly BehaviorSubject<bool> _isLoggedInSubject = new(false);
    private readonly BehaviorSubject<Account?> _loggedInAccount = new(null);
    private readonly BehaviorSubject<string?> _errorMessageSubject = new(null);

    public bool IsLoggedIn => _isLoggedIn;
    public string? ErrorMessage { get; set; }
    public IObservable<bool> WhenLoggedInChanged => _isLoggedInSubject.AsObservable();

    public IObservable<Account?> WhenAccountInChanged => _loggedInAccount.AsObservable();

    public IObservable<string?> WhenErrorMessageChanged => _errorMessageSubject.AsObservable();

    public bool Login(string username, string password)
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
        var account = _dbContext.Accounts.FirstOrDefault(u => u.Login == username && u.Password == password);
        if (account == null)
        {
            var errorMsg = "User " + username + " not found";
            LogHost.Default.Info(errorMsg);
            _errorMessageSubject.OnNext(errorMsg);
            return false;
        }

        _isLoggedIn = true;
        // Уведомляем подписчиков
        _isLoggedInSubject.OnNext(true); 
        _loggedInAccount.OnNext(account);
        _errorMessageSubject.OnNext(null);

        LogHost.Default.Info("User logged in: " + username);
        return true;
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
            var errorMsg = "Position Cannot be null, Please select a position";
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
            _isLoggedIn = true;
            _isLoggedInSubject.OnNext(true); 
            _errorMessageSubject.OnNext(null);
            _loggedInAccount.OnNext(account);
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
        if (!_isLoggedIn) return;

        _isLoggedIn = false;
        _isLoggedInSubject.OnNext(false);
        _errorMessageSubject.OnNext(null);
        LogHost.Default.Info("User logged out");
    }

    public void Dispose() => _isLoggedInSubject?.Dispose();


    /// <summary>
    /// Проверяет корректность введённых логина и пароля.
    /// </summary>
    /// <param name="username">Логин</param>
    /// <param name="password">Пароль</param>
    /// <returns>Результат валидации</returns>
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

        //if (!HasLowercase(password))
        //  return new ValidationResult(false, "Пароль должен содержать хотя бы одну строчную букву");

        // if (!HasUppercase(password))
        //    return new ValidationResult(false, "Пароль должен содержать хотя бы одну заглавную букву");

        //if (!HasDigit(password))
        //    return new ValidationResult(false, "Пароль должен содержать хотя бы одну цифру");

        return new ValidationResult(true, "OK");
    }

    /// <summary>
    /// Проверяет, что логин содержит только разрешённые символы.
    /// </summary>
    private bool IsValidUsername(string username)
    {
        foreach (char c in username)
        {
            if (!char.IsLetterOrDigit(c) && c != '.' && c != '-' && c != '_')
                return false;
        }

        return true;
    }

    private bool HasLowercase(string password) => password.Any(char.IsLower);
    private bool HasUppercase(string password) => password.Any(char.IsUpper);
    private bool HasDigit(string password) => password.Any(char.IsDigit);

    /// <summary>
    /// Простой хеш пароля
    /// </summary>
    // private string Hash(string password) =>
    //     Convert.ToBase64String(
    //         System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(password)));
    public void FlushErrorMessage() => _errorMessageSubject.OnNext(null);
}

/// <summary>
/// Результат проверки ввода.
/// </summary>
internal record ValidationResult(bool IsValid, string ErrorMessage);