using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Windows.Input;
using System.Reactive.Linq;
using System.Reactive;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;


namespace SeaMoneyApp.ViewModels;

[DataContract]
public partial class MainViewModel : ViewModelBase, IScreen
{
    private readonly ReactiveCommand<Unit, Unit> _search;
    private readonly ReactiveCommand<Unit, Unit> _login;
   
    private RoutingState _router = new RoutingState();

    public MainViewModel()
    {
        Router = new RoutingState();
        
        var canLogin = this
            .WhenAnyObservable(x => x.Router.CurrentViewModel)
            .Select(current => !(current is LoginViewModel));

        _login = ReactiveCommand.Create(
            () => { Router.Navigate.Execute(new LoginViewModel()); },
            canLogin);

        var canSearch = this
            .WhenAnyObservable(x => x.Router.CurrentViewModel)
            .Select(current => !(current is SearchViewModel));

        _search = ReactiveCommand.Create(
            () => { Router.Navigate.Execute(new SearchViewModel()); },
            canSearch);
    }
    [DataMember]
    public RoutingState Router
    {
        get => _router;
        set => this.RaiseAndSetIfChanged(ref _router, value);
    }

    public ICommand Search => _search;

    public ICommand Login => _login;
}