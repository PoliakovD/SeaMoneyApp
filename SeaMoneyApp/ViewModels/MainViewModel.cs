
using System.Runtime.Serialization;
using System.Windows.Input;
using ReactiveUI;
using SeaMoneyApp.DataAccess.Models;
using SeaMoneyApp.Extensions;

namespace SeaMoneyApp.ViewModels;

[DataContract]
public partial class MainViewModel : RoutableViewModel, IScreen
{
    private RoutingState? _router;
    
    private Account? _currentAccount;
    
    public MainViewModel()
    {
        Router ??= new RoutingState();
        HostScreen = this;
        
        LoginCommand = ReactiveCommand.Create(() => Router.NavigateAndCache<LoginViewModel>());
        SearchCommand = ReactiveCommand.Create(() => Router.NavigateAndCache<SearchViewModel>());
        RegisterCommand = ReactiveCommand.Create(() => Router.NavigateAndCache<RegistrationViewModel>());
       
    }

    [IgnoreDataMember]
    public RoutingState? Router
    {
        get => _router;
        set => this.RaiseAndSetIfChanged(ref _router, value);
    }
    [IgnoreDataMember]
    public ICommand LoginCommand { get; }

    [IgnoreDataMember]
    public ICommand SearchCommand { get; }

    [IgnoreDataMember]
    public ICommand RegisterCommand { get; }
    
    
}