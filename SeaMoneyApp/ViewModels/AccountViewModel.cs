using SeaMoneyApp.Models;
using Splat;

namespace SeaMoneyApp.ViewModels;

public class AccountViewModel: RoutableViewModel
{
    public AppSession AppSession { get; }
    
    public AccountViewModel()
    {
        AppSession = Locator.Current.GetService<AppSession>()!;
    }
}