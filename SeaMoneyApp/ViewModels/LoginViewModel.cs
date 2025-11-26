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
public class LoginViewModel : ViewModelBase, IRoutableViewModel
{
    private readonly ReactiveCommand<Unit, Unit> _login;
    [Reactive] private string? Password { get; set; }

    [DataMember]
    [Reactive]
    private string? Username{ get; set; }

    public LoginViewModel(IScreen? screen = null)
    {
        HostScreen = Locator.Current.GetService<IScreen>()!;

        var canLogin = this
            .WhenAnyValue(
                x => x.Username,
                x => x.Password,
                (user, pass) => !string.IsNullOrWhiteSpace(user) &&
                                !string.IsNullOrWhiteSpace(pass));

        _login = ReactiveCommand.CreateFromTask(
            () => Task.Delay(1000),
            canLogin);
    }

    public IScreen HostScreen { get; }

    public string UrlPathSegment => "/login";

    public ICommand Login => _login;

    
   

   
}