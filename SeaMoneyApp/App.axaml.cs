using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using ReactiveUI.Avalonia;
using SeaMoneyApp.DataAccess;
using SeaMoneyApp.Drivers;
using SeaMoneyApp.Services;
using SeaMoneyApp.Services.Authorization;
using SeaMoneyApp.Services.Logger;
using SeaMoneyApp.ViewModels;
using SeaMoneyApp.Views;
using Splat;

namespace SeaMoneyApp;

public partial class App : Application
{
    private string AppStatePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "appstate.json");

    private void SetupSuspensionForDesktop()
    {
        try
        {
            var suspension = new AutoSuspendHelper(ApplicationLifetime!);
            RxApp.SuspensionHost.CreateNewAppState = () => new MainViewModel();
            if (!File.Exists(AppStatePath))
            {
                File.Create(AppStatePath).Dispose();
            }
            var driver = new NewtonsoftJsonSuspensionDriver(AppStatePath);
            RxApp.SuspensionHost.SetupDefaultSuspendResume(driver);
            suspension.OnFrameworkInitializationCompleted();
        }
        catch (Exception ex)
        {
            LogHost.Default.Error(ex, "Failed to setup app state");
        }
        
    }

    private async void SaveAppStateManually()
    {
        try
        {
            var state = RxApp.SuspensionHost.GetAppState<MainViewModel>();
            var driver = new NewtonsoftJsonSuspensionDriver(AppStatePath);
            await driver.SaveState(state);
            LogHost.Default.Info( "Saved app state successfully"); // Логируем успешный сохране
        }
        catch (Exception ex)
        {
            LogHost.Default.Error(ex, "Failed to save app state");
        }
    }
    
    private Task LoadAppStateManuallyAsync()
    {
        return Task.Run(async () =>
        {
            try
            {
                if (!File.Exists(AppStatePath))
                {
                    RxApp.SuspensionHost.CreateNewAppState = () => new MainViewModel();
                    return;
                }

                var driver = new NewtonsoftJsonSuspensionDriver(AppStatePath);
                var state = await driver.LoadState() as MainViewModel;

                if (state?.Router == null)
                {
                    LogHost.Default.Warn("Router is null after deserialization. Recreating MainViewModel.");
                    state = new MainViewModel();
                }

                RxApp.SuspensionHost.AppState = state;
                LogHost.Default.Info("Loaded app state successfully");
            }
            catch (Exception ex)
            {
                LogHost.Default.Error(ex, "Failed to load app state");
                RxApp.SuspensionHost.AppState = new MainViewModel();
            }
        });
    }

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        
        Localization.Localization.Culture = new CultureInfo("ru-RU");
        
        LoggerSetup.SetupLogger(LogLevel.Debug); // Регистрируем логгер
        // Регистрируем сервис database context как Singleton
        Locator.CurrentMutable.RegisterLazySingleton<DataBaseContext>(() => DataBaseContextFactory.CreateWithDefaultConnectionString());
        Locator.CurrentMutable.RegisterLazySingleton<IAuthorizationService>(() => new AuthorizationService());
        
        
        
        // Регистрируем сервис авторизации как Singleton
        Locator.CurrentMutable.RegisterLazySingleton<IAuthorizationService>
            (() => new AuthorizationService());
        var screen = new MainViewModel();
        
        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
            {
                // LoadAppStateManuallyAsync().GetAwaiter().GetResult();
                // var screen = RxApp.SuspensionHost.GetAppState<MainViewModel>();
                
                Locator.CurrentMutable.RegisterConstant<IScreen>(screen);

                desktop.MainWindow = new MainWindow { DataContext = screen };
               // screen.Router.Navigate.Execute(new LoginViewModel());
                desktop.ShutdownRequested += (sender, e) => SaveAppStateManually();
                break;
            }
            case ISingleViewApplicationLifetime singleView:
            {
                // На Android — вручную загружаем состояние
                //LoadAppStateManuallyAsync().GetAwaiter().GetResult();

                //var screen = RxApp.SuspensionHost.GetAppState<MainViewModel>();
                
                Locator.CurrentMutable.RegisterConstant<IScreen>(screen);

                singleView.MainView = new MainView { DataContext = screen };
               // singleView.MainView.DetachedFromVisualTree += async (sender, e) =>
               // {
               //     // Сохраняем состояние вручную
               //     await new NewtonsoftJsonSuspensionDriver(AppStatePath)
                //        .SaveState(RxApp.SuspensionHost.GetAppState<MainViewModel>());
                //};
                break;
            }
        }
        
        LogHost.Default.Info("Initialized application successfully");
        
        Locator.CurrentMutable.Register<IViewFor<SearchViewModel>>(() => new SearchView());
        Locator.CurrentMutable.Register<IViewFor<LoginViewModel>>(() => new LoginView());
        Locator.CurrentMutable.Register<IViewFor<RegistrationViewModel>>(() => new RegistrationView());

        Locator.Current.GetService<IScreen>()?.Router.Navigate.Execute(new LoginViewModel(screen));
        
        LogHost.Default.Info("Registered views successfully");
        
        
        //Test
        var context = Locator.Current.GetService<DataBaseContext>();
        context?.Database.EnsureCreated();
        var positions = context?.Positions.ToList();
        var str=context.Database.GetConnectionString();
        LogHost.Default.Debug($"Loaded positions count: {positions?.Count}");
        LogHost.Default.Debug($"Connection string is: {str}");
        //Test
        
        
        
        
        base.OnFrameworkInitializationCompleted();
    }
}

