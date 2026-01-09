using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUI.Avalonia;
using SeaMoneyApp.ViewModels;
using SeaMoneyApp.ViewModels.OveralViewModels;

namespace SeaMoneyApp.Views.OverallViews;

public partial class AppSettingsView : ReactiveUserControl<AppSettingsViewModel>
{
    public AppSettingsView()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
    }

   
}