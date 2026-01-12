using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUI.Avalonia;
using SeaMoneyApp.ViewModels.OverallViewModels;

namespace SeaMoneyApp.Views.OverallViews;

public partial class AdminPanelMainView :  ReactiveUserControl<AdminPanelMainViewModel>
{
    public AdminPanelMainView()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
    }
}