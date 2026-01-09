using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUI.Avalonia;
using SeaMoneyApp.ViewModels.OveralViewModels;

namespace SeaMoneyApp.Views.OverallViews;

public partial class AccountView :   ReactiveUserControl<AccountViewModel>
{
    public AccountView()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
    }
}