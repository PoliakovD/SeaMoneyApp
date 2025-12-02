using Android.App;
using Android.Content.PM;
using Android.OS;
using Avalonia;
using Avalonia.Android;
using ReactiveUI;
using ReactiveUI.Avalonia;
using SeaMoneyApp.DataAccess;
using SeaMoneyApp.Extensions;
using SeaMoneyApp.ViewModels;
using Splat;

namespace SeaMoneyApp.Android;

[Activity(
    Label = "SeaMoneyApp.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    RoundIcon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        
        // Устанавливаем инициализатор ДО любого доступа к БД
        DataBaseContextFactory.SetInitializer(new AndroidDatabaseInitializer());
        return base.CustomizeAppBuilder(builder)
            .WithInterFont()
            .UseReactiveUI();
    }
    public override void OnBackPressed()
    {
        // Получаем текущий Router
        var currentRouter = (Locator.Current?.GetService<IScreenBackCommand>()!).Router;
        if (currentRouter?.NavigationStack.Count > 1)
        {
            //LogHost.Default.Debug($"Вызван метод из IScreenBackCommand, в стеке навигации {currentRouter?.NavigationStack.Count} представлений");
            // Если есть куда возвращаться — делаем NavigateBack
            currentRouter.NavigateBack.Execute();
        }
        else
        { 
            //LogHost.Default.Debug("Вызван метод из Android");
           base.OnBackPressed();
        }
    }
   
}