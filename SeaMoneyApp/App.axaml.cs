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
using SeaMoneyApp.Extensions;
using SeaMoneyApp.Models;
using SeaMoneyApp.Services;
using SeaMoneyApp.Services.Authorization;
using SeaMoneyApp.Services.Logger;
using SeaMoneyApp.Services.UpdateCources;
using SeaMoneyApp.ViewModels;
using SeaMoneyApp.Views;
using SeaMoneyApp.Views.OverallViews;
using Splat;

namespace SeaMoneyApp;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        Localization.Localization.Culture = new CultureInfo("ru-RU");

        LoggerSetup.SetupLogger(LogLevel.Debug); // Регистрируем логгер

        // Регистрируем сервис database context
        Locator.CurrentMutable.RegisterConstant(DataBaseContextFactory.CreateWithDefaultConnectionString());

        // Регистрируем сервис авторизации
        Locator.CurrentMutable.RegisterConstant<IAuthorizationService>
            (new AuthorizationService());

        // Регистрируем сервис загрузки курсов как Singleton
        Locator.CurrentMutable.RegisterLazySingleton
            (() => new UpdateCourcesService());

        // Устанавливаем глобальный ViewLocator
        Locator.CurrentMutable.RegisterViewsForViewModels(typeof(App).Assembly);
        
        var appSession = new AppSession();
        Locator.CurrentMutable.RegisterConstant(appSession);


        var screen = new MainViewModel();

       

        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
            {
                Locator.CurrentMutable.RegisterConstant<IScreen>(screen);
                desktop.MainWindow = new MainWindow { DataContext = screen };

#if DEBUG
                // Вручную открываем DevTools при запуске (если нужно)
                DevToolsExtensions.AttachDevTools(desktop.MainWindow);
#endif
                break;
            }
            case ISingleViewApplicationLifetime singleView:
            {
                Locator.CurrentMutable.RegisterConstant<IScreen>(screen);
                singleView.MainView = new MainView { DataContext = screen };
                break;
            }
        }

        LogHost.Default.Info("Initialized application successfully");

        var router = Locator.Current.GetService<IScreen>()?.Router;

        if (appSession.CurrentAccount is null)
        {
            // Регистрируем screen как команду для перемещения назад как синглтон
            Locator.CurrentMutable.RegisterConstant<IScreenBackCommand>(screen);
            router.NavigateAndCache<LoginViewModel>();
        }
        else
        {
            var overalViewModel = new OverallViewModel();
            
            // Регистрируем screen как команду для перемещения назад как синглтон
            Locator.CurrentMutable.RegisterConstant<IScreenBackCommand>(overalViewModel);
            router.NavigateAndCache<OverallViewModel>(overalViewModel);
        }
        // DEBUG
        
        LogHost.Default.Info($"appSession Culture:  {appSession.Culture}");
        
        // DEBUG
        
        
        
       

        LogHost.Default.Info("Registered views successfully");


        base.OnFrameworkInitializationCompleted();
    }
}