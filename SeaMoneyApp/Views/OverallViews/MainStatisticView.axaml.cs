
using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUI.Avalonia;
using SeaMoneyApp.ViewModels.OveralViewModels;

namespace SeaMoneyApp.Views.OverallViews;

public partial class MainStatisticView :  ReactiveUserControl<MainStatisticViewModel>
{
    public MainStatisticView()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
    }
}