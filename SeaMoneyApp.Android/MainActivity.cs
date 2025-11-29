using Android.App;
using Android.Content.PM;
using Android.OS;
using Avalonia;
using Avalonia.Android;
using ReactiveUI.Avalonia;
using SeaMoneyApp.DataAccess;

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
   
}