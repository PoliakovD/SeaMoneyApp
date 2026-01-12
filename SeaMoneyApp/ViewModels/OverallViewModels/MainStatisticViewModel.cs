using System;
using System.Linq;
using System.Reactive;
using ReactiveUI;
using SeaMoneyApp.Extensions;
using SeaMoneyApp.Models;
using SeaMoneyApp.Services;
using SeaMoneyApp.ViewModels.MainStatisticViewModels;

namespace SeaMoneyApp.ViewModels.OverallViewModels;

public class MainStatisticViewModel : RoutableViewModel, IScreen
{
    private RoutingState _router;
    public WageControlService WageService { get; }
    
    private ContractStatistic _selectedContractStatistic;
    public ContractStatistic? SelectedContractStatistic {
        get => _selectedContractStatistic;
        set => this.RaiseAndSetIfChanged(ref _selectedContractStatistic, value);
    }
    
    public RoutingState? Router
    {
        get => _router;
        set => this.RaiseAndSetIfChanged(ref _router, value);
    }
    public MainStatisticViewModel()
    {
        WageService = new WageControlService();
        SelectedContractStatistic = WageService.ContractWageLogsDictionary.Values.FirstOrDefault();
        Router ??= new RoutingState();
        if (SelectedContractStatistic is not null)
        {
            this.WhenAnyValue(x => x.SelectedContractStatistic)
                .Subscribe((contract) =>
                {
                    if(contract is not null)
                        Router.NavigateAndNotCache<ContractStatisticViewModel>(new ContractStatisticViewModel(contract));
                });
        }
       
        
    }

    
}