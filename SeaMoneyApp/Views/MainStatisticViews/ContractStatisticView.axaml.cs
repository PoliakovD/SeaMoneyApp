using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUI.Avalonia;
using SeaMoneyApp.Models;
using SeaMoneyApp.ViewModels.MainStatisticViewModels;

namespace SeaMoneyApp.Views.MainStatisticViews;

public partial class ContractStatisticView:  ReactiveUserControl<ContractStatisticViewModel>
{

    
    public ContractStatisticView()
    {
       
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
    }
}