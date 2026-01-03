using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUI.Avalonia;
using SeaMoneyApp.ViewModels;

namespace SeaMoneyApp.Views.OverallViews;

public partial class CoursesView : ReactiveUserControl<CoursesViewModel>
{
    public CoursesView()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
    }

    
}