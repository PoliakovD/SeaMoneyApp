using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUI.Avalonia;
using SeaMoneyApp.DataAccess.Models;
using SeaMoneyApp.ViewModels;

namespace SeaMoneyApp.Views;

public partial class RegistrationView : ReactiveUserControl<RegistrationViewModel>
{
    public RegistrationView()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
    }
}