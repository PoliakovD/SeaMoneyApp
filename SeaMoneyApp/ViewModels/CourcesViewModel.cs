using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using SeaMoneyApp.DataAccess.Models;
using SeaMoneyApp.Services.UpdateCources;
using Splat;

namespace SeaMoneyApp.ViewModels;

public class CourcesViewModel : RoutableViewModel
{
    public ObservableCollection<ChangeRubToDollar> Cources { get; set; } = [];
    private string? _errorMessage;

    public ReactiveCommand<Unit, Unit> LoadCourcesFromHttpCommand { get; }
    
    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    private readonly UpdateCourcesService _updateService;
  
    public CourcesViewModel()
    {
        _updateService = Locator.Current.GetService<UpdateCourcesService>() 
            ?? throw new ArgumentNullException(nameof(_updateService));
        
        LoadCourcesFromHttpCommand = ReactiveCommand.CreateFromTask(
            async () =>
            {
                Cources.Clear();
                LogHost.Default.Debug("LoadCourcesFromHttpCommand started");
                await _updateService.LoadCourcesAsync(Cources);
                LogHost.Default.Debug("LoadCourcesFromHttpCommand finished");
            }
        );
        
        // Подписываемся на изменения ошибки
        _updateService.WhenErrorMessageChanged
            .BindTo(this, vm => vm.ErrorMessage);

        
    }
}