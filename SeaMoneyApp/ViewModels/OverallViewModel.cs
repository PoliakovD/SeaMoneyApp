using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Windows.Input;
using System.Reactive.Linq;
using System.Reactive;
using ReactiveUI;
using SeaMoneyApp.DataAccess.Models;
using SeaMoneyApp.Extensions;
using SeaMoneyApp.Services.Authorization;
using Splat;

namespace SeaMoneyApp.ViewModels;

[DataContract]
public class OverallViewModel : RoutableViewModel, IScreenBackCommand
{
    public ICommand ToLogsCommand { get; }
    public ICommand ToAccountCommand { get; }
    public ICommand ToAppSettingsCommand { get; }
    public ICommand ToCoursesCommand { get; }
    
    public ICommand ToIncomesCommand { get; }
    public ICommand ToContractsCommand { get; }
    private RoutingState? _router;

    [IgnoreDataMember]
    public RoutingState? Router
    {
        get => _router;
        set => this.RaiseAndSetIfChanged(ref _router, value);
    }

    public OverallViewModel()
    {
        Router ??= new RoutingState();

        //Router?.NavigateAndCache<AccountViewModel>();

        ToLogsCommand = ReactiveCommand.Create(() => { Router.NavigateAndCache<LogsViewModel>(); });
       
        ToAccountCommand = ReactiveCommand.Create(() => { Router.NavigateAndCache<AccountViewModel>();});
        
        ToCoursesCommand = ReactiveCommand.Create(() => { Router.NavigateAndCache<CoursesViewModel>(); });
        
        ToAppSettingsCommand= ReactiveCommand.Create(() => { Router.NavigateAndCache<AppSettingsViewModel>(); });

        ToContractsCommand = ReactiveCommand.Create(() => Router.NavigateAndCache<ContractsViewModel>());
        
        ToIncomesCommand= ReactiveCommand.Create(() => Router.NavigateAndCache<IncomesViewModel>());
    }
}