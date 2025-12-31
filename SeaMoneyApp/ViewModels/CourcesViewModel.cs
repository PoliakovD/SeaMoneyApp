using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using ReactiveUI;
using SeaMoneyApp.DataAccess;
using SeaMoneyApp.DataAccess.Models;
using SeaMoneyApp.Services.UpdateCources;
using Splat;

namespace SeaMoneyApp.ViewModels;

public partial class CourcesViewModel : RoutableViewModel
{
    private const int HttpTimeOut = 100000;
    public ObservableCollection<ChangeRubToDollar> Cources { get; set; } = [];
    
    private readonly DataBaseContext _dbContext = Locator.Current.GetService<DataBaseContext>()!;
    private string? _errorMessage;

    private IObservable<bool> htmlRunning;
    private CancellationTokenSource _ctsHttp;
    public ReactiveCommand<Unit, Unit> UpdateCourcesFromHttpCommand { get; }
    public ReactiveCommand<Unit, Unit> StopLoadFromHttpCommand { get; }
    
    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    private readonly UpdateCourcesService _updateService;
  
    public  CourcesViewModel()
    {
        // загрузка курсов из бд
        LoadFromDb();
        
        _updateService = Locator.Current.GetService<UpdateCourcesService>() 
            ?? throw new ArgumentNullException(nameof(_updateService));
        
        htmlRunning = _updateService.WhenCanStartChanged;
        
        UpdateCourcesFromHttpCommand = ReactiveCommand.CreateFromTask(
            async () =>
            {
                _ctsHttp = new CancellationTokenSource(HttpTimeOut);
                var cToken = _ctsHttp.Token;
                try
                {
                    cToken.ThrowIfCancellationRequested();
                    Cources.Clear();
                    LogHost.Default.Debug("LoadCourcesFromHttpCommand started");
                    await _updateService.LoadCourcesAsync(Cources, cToken);
                    LogHost.Default.Debug("LoadCourcesFromHttpCommand finished");
                    
                }
                catch (OperationCanceledException)
                {
                    LogHost.Default.Debug("LoadCourcesFromHttpCommand cancelled or timed out");
                }
                catch (Exception ex)
                {
                    LogHost.Default.Error(ex, "LoadCourcesFromHttpCommand error");
                }
            },
            canExecute: htmlRunning
        );
        
       
        StopLoadFromHttpCommand = ReactiveCommand.Create(()=>_ctsHttp?.Cancel(),
            canExecute: UpdateCourcesFromHttpCommand.IsExecuting);
        
        // Подписываемся на изменения ошибки
        _updateService.WhenErrorMessageChanged
            .BindTo(this, vm => vm.ErrorMessage);
       
        // Инициализация Chart
        ChartInit();
    }

    private void LoadFromDb()
    {
        foreach (var cources in _dbContext.GetAllCources())
        {
            Cources.Add(cources);
        }
    }
    
}