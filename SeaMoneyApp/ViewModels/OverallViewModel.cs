using System.Windows.Input;
using ReactiveUI;
using SeaMoneyApp.Extensions;
using SeaMoneyApp.ViewModels.OveralViewModels;

namespace SeaMoneyApp.ViewModels;

public class OverallViewModel : RoutableViewModel, IScreenBackCommand
{
    public ICommand ToLogsCommand { get; }
    public ICommand ToAccountCommand { get; }
    public ICommand ToAppSettingsCommand { get; }
    public ICommand ToCoursesCommand { get; }
    
    public ICommand ToIncomesCommand { get; }
    public ICommand ToContractsCommand { get; }
    private RoutingState? _router;


    public RoutingState? Router
    {
        get => _router;
        set => this.RaiseAndSetIfChanged(ref _router, value);
    }

    public OverallViewModel()
    {
        Router ??= new RoutingState();
        

        ToLogsCommand = ReactiveCommand.Create(() => { Router.NavigateAndCache<LogsViewModel>(); });
       
        ToAccountCommand = ReactiveCommand.Create(() => { Router.NavigateAndCache<AccountViewModel>();});
        
        ToCoursesCommand = ReactiveCommand.Create(() => { Router.NavigateAndCache<CoursesViewModel>(); });
        
        ToAppSettingsCommand= ReactiveCommand.Create(() => { Router.NavigateAndNotCache<AppSettingsViewModel>(); });

        ToContractsCommand = ReactiveCommand.Create(() => Router.NavigateAndNotCache<ContractsViewModel>());
        
        ToIncomesCommand= ReactiveCommand.Create(() => Router.NavigateAndNotCache<IncomesViewModel>());
    }
}