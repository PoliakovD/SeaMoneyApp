using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using ReactiveUI;
using ReactiveUI.Avalonia;
using SeaMoneyApp.ViewModels;
using Splat;

namespace SeaMoneyApp.Views.OverallViews;

public partial class LogsView : ReactiveUserControl<LogsViewModel>
{
    public LogsView()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
        
    }
    
   
}