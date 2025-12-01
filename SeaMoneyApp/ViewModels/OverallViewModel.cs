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
public class OverallViewModel: RoutableViewModel
{
    public ICommand ToLogsCommand { get; }
    
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
        ToLogsCommand = ReactiveCommand.Create(() => Router.NavigateAndCache<LogsViewModel>());
        if (Router != null && Router.NavigationStack.Count > 0)
            Router.NavigateAndCache<LogsViewModel>();
    }
    
   
}