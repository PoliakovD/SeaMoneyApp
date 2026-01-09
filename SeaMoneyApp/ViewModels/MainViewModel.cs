
using System.Windows.Input;
using ReactiveUI;
using SeaMoneyApp.Extensions;

namespace SeaMoneyApp.ViewModels;

public class MainViewModel : ReactiveObject, IScreen, IScreenBackCommand
{
    private RoutingState? _router;
    public MainViewModel()
    {
        Router ??= new RoutingState();
        
        LoginCommand = ReactiveCommand.Create(() => Router.NavigateAndCache<LoginViewModel>());
        SearchCommand = ReactiveCommand.Create(() => Router.NavigateAndCache<OverallViewModel>());
        RegisterCommand = ReactiveCommand.Create(() => Router.NavigateAndCache<RegistrationViewModel>());
    }

    public RoutingState? Router
    {
        get => _router;
        set => this.RaiseAndSetIfChanged(ref _router, value);
    }
    public ICommand LoginCommand { get; }
    
    public ICommand SearchCommand { get; }

    public ICommand RegisterCommand { get; }
    
}