using System;
using System.Threading.Tasks;
using SeaMoneyApp.DataAccess.Models;

namespace SeaMoneyApp.Services.Authorization;

/// <summary>
/// Сервис управления авторизацией пользователя.
/// </summary>
public interface IAuthorizationService
{
    /// <summary>
    /// Текущее состояние авторизации.
    /// </summary>
    bool IsLoggedIn { get; }

    /// <summary>
    /// Реактивный поток, уведомляющий об изменениях состояния авторизации.
    /// </summary>
    IObservable<bool> WhenLoggedInChanged { get; }
    
    IObservable<Account?> WhenAccountInChanged { get; }
    IObservable<string?> WhenErrorMessageChanged { get; }
    /// <summary>
    /// Асинхронная попытка входа.
    /// </summary>
    /// <param name="username">Имя пользователя</param>
    /// <param name="password">Пароль</param>
    /// <returns>Успех или ошибка</returns>
    bool Login(string username, string password);
    
    bool Register(string? login, string? password, Position? position, short? toursInRank);
    /// <summary>
    /// Выход из аккаунта.
    /// </summary>
    void Logout();

    void FlushErrorMessage();
}