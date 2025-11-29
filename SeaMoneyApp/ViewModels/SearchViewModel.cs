using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Windows.Input;
using System.Reactive.Linq;
using System.Reactive;
using ReactiveUI;
using SeaMoneyApp.DataAccess.Models;
using SeaMoneyApp.Services.Authorization;
using Splat;

namespace SeaMoneyApp.ViewModels;
[DataContract]
public class SearchViewModel: ViewModelBase, IRoutableViewModel
{
    private readonly ReactiveCommand<Unit, Unit> _search;
    private string? _searchQuery;
    private Account? _currentAccount = null;
    public Account? CurrentAccount
    {
        get => _currentAccount;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentAccount, value);
           
        }
    }

    public SearchViewModel(IScreen? screen = null)
    {
        HostScreen = screen ?? Locator.Current.GetService<IScreen>()!;

        var canSearch = this
            .WhenAnyValue(x => x.SearchQuery)
            .Select(query => !string.IsNullOrWhiteSpace(query));

        _search = ReactiveCommand.CreateFromTask(
            () => Task.Delay(1000),
            canSearch);
        var authService = Locator.Current.GetService<IAuthorizationService>();
        // Подписываемся на изменения Пользлвателся
        authService.WhenAccountInChanged
            .BindTo(this, vm => vm.CurrentAccount);
    }

    public IScreen HostScreen { get; }

    public string UrlPathSegment => "/search";

    public ICommand Search => _search;

    [DataMember]
    public string? SearchQuery
    {
        get => _searchQuery;
        set => this.RaiseAndSetIfChanged(ref _searchQuery, value);
    }
}