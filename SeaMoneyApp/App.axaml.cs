using System;
using System.Globalization;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUI.Avalonia;
using SeaMoneyApp.Drivers;
using SeaMoneyApp.ViewModels;
using SeaMoneyApp.Views;
using Splat;
namespace SeaMoneyApp;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using SeaMoneyApp.Drivers;
using SeaMoneyApp.ViewModels;
using SeaMoneyApp.Views;
using Splat;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    // public override void OnFrameworkInitializationCompleted()
    // {
    //     if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    //     {
    //         desktop.MainWindow = new MainWindow
    //         {
    //             DataContext = new MainViewModel()
    //         };
    //     }
    //     else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
    //     {
    //         singleViewPlatform.MainView = new MainView
    //         {
    //             DataContext = new MainViewModel()
    //         };
    //     }
    //     Localization.Localization.Culture = new CultureInfo("ru-RU");
    //     base.OnFrameworkInitializationCompleted();
    // }
    public override void OnFrameworkInitializationCompleted()
    {
        var suspension = new AutoSuspendHelper(ApplicationLifetime!);
        RxApp.SuspensionHost.CreateNewAppState = () => new MainViewModel();
        RxApp.SuspensionHost.SetupDefaultSuspendResume(new NewtonsoftJsonSuspensionDriver("appstate.json"));
        suspension.OnFrameworkInitializationCompleted();

        Locator.CurrentMutable.RegisterConstant<IScreen>(RxApp.SuspensionHost.GetAppState<MainViewModel>());
        Locator.CurrentMutable.Register<IViewFor<SearchViewModel>>(() => new SearchView());
        Locator.CurrentMutable.Register<IViewFor<LoginViewModel>>(() => new LoginView());
            
        new MainView { DataContext = Locator.Current.GetService<IScreen>() }.Show();
        base.OnFrameworkInitializationCompleted();
    }
}