using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ReactiveUI;
using SeaMoneyApp.DataAccess;
using SeaMoneyApp.DataAccess.Models;
using SeaMoneyApp.Services.Authorization;
using SeaMoneyApp.Services.JsonService;
using Splat;

namespace SeaMoneyApp.Models;


public class AppSession : ReactiveObject,IDisposable, IAsyncDisposable
{
    private readonly DataBaseContext _dbContext = Locator.Current.GetService<DataBaseContext>()!;
    private Account? _currentAccount;
    private bool? _isAdmin;
    private bool _rememberMe = false;
    public bool? IsAdmin 
    {
        get => _isAdmin;
        set => this.RaiseAndSetIfChanged(ref _isAdmin, value);
    }

    public bool RememberMe
    {
        get => _rememberMe;
        set => this.RaiseAndSetIfChanged(ref _rememberMe, value);
    }

    public Account? CurrentAccount
    {
        get => _currentAccount;
        set => this.RaiseAndSetIfChanged(ref _currentAccount, value);
    }
    

    public bool IsLoggedIn => CurrentAccount is not null;
    

    private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.All
    };

    private static string GetAuthFilePath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appData, "SeaMoneyApp");
        LogHost.Default.Info($"appFolder Path: {Path.Combine(appFolder, "auth.json")}");

        Directory.CreateDirectory(appFolder);
        return Path.Combine(appFolder, "auth.json");
    }

    public AppSession()
    {
        LogHost.Default.Info("Initializing AppSession");
        StartListeningToAuth();

        RestoreSession();
        
    }
    public void StartListeningToAuth()
    {
        var authService = Locator.Current.GetService<IAuthorizationService>();
        authService.WhenAccountInChanged
            .BindTo(this, vm => vm.CurrentAccount);

        authService.WhenRememberMeChanged
            .BindTo(this, vm => vm.RememberMe);

        authService.WhenAccountInChanged
            .Subscribe(account => IsAdmin = account?.Login == "admin");
    }
    public bool RestoreSession()
    {
        LogHost.Default.Info("Restoring session");
        try
        {
            var authFilePath = GetAuthFilePath();   
            if (!File.Exists(authFilePath)) return false;

            var json = File.ReadAllText(authFilePath);
            
            var session = JsonService.Load<AppSessionRestore>(authFilePath);
            
            LogHost.Default.Info("Restored session: " + json);
            //var saved = JsonConvert.DeserializeObject<AppSession>(json, _settings);
            
            if (session.SavedRememberMe == false) return false;
            
            var account = _dbContext.Accounts
                .Include(a => a.Position)
                .FirstOrDefault(u => u.Login == session.SavedLogin);

            if (account != null)
            {
                CurrentAccount = account;
                LogHost.Default.Info("Auto-login succeeded for: " + account.Login);
                return true;
            }
            else LogHost.Default.Info("Auto-login failed for: " + session.SavedLogin);
            
        }
        catch (Exception ex)
        {
            LogHost.Default.Error(ex, "Failed to restore session");
        }
        return false;
    }
    public void SaveSession()
    {
        LogHost.Default.Info("Saving session");
        if (RememberMe)
        {
            try
            {
                ClearSavedSession();
                
                var authFilePath = GetAuthFilePath();   
                var savedSession = new AppSessionRestore()
                {
                    SavedLogin = CurrentAccount.Login,
                    SavedRememberMe = RememberMe
                };
                var json = JsonService.Save(savedSession, authFilePath);
                //var json = JsonConvert.SerializeObject(session, Formatting.Indented, _settings);
                LogHost.Default.Info("Saving session");

                LogHost.Default.Debug(json);

                File.WriteAllText(authFilePath, json);
            }
            catch (Exception ex)
            {
                LogHost.Default.Error(ex, "Failed to save session");
            }
        }
    }
    public async Task SaveSessionAsync()
    {
        LogHost.Default.Info("Saving session async");
        if (RememberMe)
        {
            try
            {
                ClearSavedSession();
                
                var authFilePath = GetAuthFilePath();   
                var savedSession = new AppSessionRestore()
                {
                    SavedLogin = CurrentAccount.Login,
                    SavedRememberMe = RememberMe
                };
                var json = await JsonService.SaveAsync(savedSession, authFilePath);
                //var json = JsonConvert.SerializeObject(session, Formatting.Indented, _settings);

                LogHost.Default.Debug(json);

                //File.WriteAllText(authFilePath, json);
            }
            catch (Exception ex)
            {
                LogHost.Default.Error(ex, "Failed to save session async");
            }
        }
    }
    public void ClearSavedSession()
    {
        try
        {
            var authFilePath = GetAuthFilePath();
            if (File.Exists(authFilePath))
                File.Delete(authFilePath);
        }
        catch (Exception ex)
        {
            LogHost.Default.Error(ex, "Failed to clear saved session");
        }
    }

    public void Dispose()
    {
        SaveSession();
        _dbContext.Dispose();
    }
    
    private record AppSessionRestore
    {
        public string? SavedLogin;
        public bool SavedRememberMe;
    }

    public async ValueTask DisposeAsync()
    {
        await _dbContext.DisposeAsync();
    }
}

