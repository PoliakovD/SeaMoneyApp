using System.Reactive.Linq;
using System.Reactive;
using System.Runtime.Serialization;
using System.Windows.Input;
using ReactiveUI;
using SeaMoneyApp.DataAccess.Models;
using Splat;

namespace SeaMoneyApp.ViewModels;

[DataContract]
public partial class MainViewModel : ViewModelBase, IScreen
{
    private RoutingState _router;
    
    private LoginViewModel? _loginViewModel;
    private RegistrationViewModel? _registrationViewModel;
    private SearchViewModel? _searchViewModel;
    
    private Account? _currentAccount;
    public LoginViewModel? LoginViewModel
    {
        get => _loginViewModel;
    }
    
    public MainViewModel()
    {
        Router ??= new RoutingState();
        
        // Создаём команды навигации
        LoginCommand = ReactiveCommand.Create(EnsureLoginViewModelAndNavigate);
        SearchCommand = ReactiveCommand.Create(EnsureSearchViewModelAndNavigate);
        RegisterCommand = ReactiveCommand.Create(EnsureRegistrationViewModelAndNavigate);
        // Команда "Назад" — доступна, если есть куда возвращаться
    }

    [IgnoreDataMember]
    public RoutingState Router
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
    
    private void EnsureLoginViewModelAndNavigate()
    {
        _loginViewModel ??= new LoginViewModel();
        Router.Navigate.Execute(_loginViewModel);
    }

    private void EnsureSearchViewModelAndNavigate()
    {
        _searchViewModel ??= new SearchViewModel();
        Router.Navigate.Execute(_searchViewModel);
    }

    private void EnsureRegistrationViewModelAndNavigate()
    {
        _registrationViewModel ??= new RegistrationViewModel();
        Router.Navigate.Execute(_registrationViewModel);
    }
}