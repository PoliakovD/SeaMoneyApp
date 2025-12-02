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
using SeaMoneyApp.Extensions;
using SeaMoneyApp.Models;
using SeaMoneyApp.Services;
using SeaMoneyApp.Services.Authorization;
using SeaMoneyApp.Services.Logger;
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
        
        // Регистрируем сервис database context как Singleton
        Locator.CurrentMutable.RegisterLazySingleton(DataBaseContextFactory.CreateWithDefaultConnectionString);
        
        // Регистрируем сервис авторизации как Singleton
        Locator.CurrentMutable.RegisterLazySingleton<IAuthorizationService>
            (() => new AuthorizationService());
        
        // Регистрируем AppSession как синглтон
        Locator.CurrentMutable.RegisterLazySingleton(() => new AppSession());
        
        var screen = new MainViewModel();
        
        // Регистрируем screen как команду для перемещения назад как синглтон
        Locator.CurrentMutable.RegisterConstant<IScreenBackCommand>(screen);
        
        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
            {
                Locator.CurrentMutable.RegisterConstant<IScreen>(screen);
                desktop.MainWindow = new MainWindow { DataContext = screen };
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
        
        Locator.CurrentMutable.Register<IViewFor<OverallViewModel>>(() => new OverallView());
        Locator.CurrentMutable.Register<IViewFor<LoginViewModel>>(() => new LoginView());
        Locator.CurrentMutable.Register<IViewFor<RegistrationViewModel>>(() => new RegistrationView());
        
        Locator.CurrentMutable.Register<IViewFor<LogsViewModel>>(() => new LogsView());
        
        Locator.Current.GetService<IScreen>()?.Router.NavigateAndCache<LoginViewModel>();
        
        LogHost.Default.Info("Registered views successfully");
        
        base.OnFrameworkInitializationCompleted();
    }
}

