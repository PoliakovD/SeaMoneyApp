using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUI.Avalonia;
using SeaMoneyApp.ViewModels.OveralViewModels;

namespace SeaMoneyApp.Views.OverallViews;

public partial class IncomesView : ReactiveUserControl<IncomesViewModel>
{
    public IncomesView()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
    }
}