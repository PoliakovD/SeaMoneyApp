using System;

namespace SeaMoneyApp.Models;

//TODO Данные приложения, которые можно безопасно сериализовать. 
/// <summary>
/// Данные приложения, которые можно безопасно сериализовать.
/// </summary>
public class AppData
{
    public string LastUsername { get; set; } = string.Empty;
    public bool IsLoggedIn { get; set; }
    public DateTime? LastLogin { get; set; }
    
}