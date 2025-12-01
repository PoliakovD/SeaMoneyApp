using System.Runtime.Serialization;
using System.Windows.Input;
using ReactiveUI;
using SeaMoneyApp.Extensions;

namespace SeaMoneyApp.ViewModels;

public class AdminPanelMainViewModel : RoutableViewModel
{
    [IgnoreDataMember]
    public ICommand ToLogsCommand { get; }

    public AdminPanelMainViewModel()
    {
        ToLogsCommand= ReactiveCommand.Create(
            () => HostScreen.Router.NavigateAndCache<LogsViewModel>());
    }
}