using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Collections;
using LiveChartsCore.SkiaSharpView.Avalonia;
using ReactiveUI;
using SeaMoneyApp.DataAccess;
using SeaMoneyApp.DataAccess.Models;
using SeaMoneyApp.Models;
using Splat;

namespace SeaMoneyApp.Services;

public class WageControlService: ReactiveObject
{
    public ObservableCollection<Contract> UserContracts { get; set; } = [];
    public ObservableCollection<WageLog> WageLogs { get; set; } = [];
    public AvaloniaDictionary<Contract,ContractStatistic> ContractWageLogsDictionary { get; set; } = [];
    
 
    
    public decimal TotalAchivedInRubles { get; set; } = 0.0m;
    public decimal TotalAchivedInDollars { get; set; } = 0.0m;
    
    private readonly DataBaseContext _dbContext = Locator.Current.GetService<DataBaseContext>()!;
    private readonly AppSession _appSession = Locator.Current.GetService<AppSession>()!;
    

    public WageControlService()
    {
        LoadUserContracts();
        
        InitWageLogs();
        
        InitContractWageLogs();
        
    }
    private void LoadUserContracts()
    {
         foreach (var contract in _dbContext.GetUserContractsIEnumerable(_appSession.CurrentAccount!)!)
         {
             UserContracts.Add(contract);
         }
    }

    private void InitWageLogs()
    {
        foreach (var wageLog in _dbContext.GetUserWageLogsEnumerable(_appSession.CurrentAccount!)!)
        {
            WageLogs.Add(wageLog);
        }
    }

    private void InitContractWageLogs()
    {
        foreach (var contract in UserContracts)
        {
            ContractWageLogsDictionary.Add(contract, new ContractStatistic(contract,WageLogs));
            
            TotalAchivedInRubles+=ContractWageLogsDictionary[contract].CurrentAchivedWageRub;
            TotalAchivedInDollars+=ContractWageLogsDictionary[contract].CurrentAchivedWageDollar;
        }
    }
    
}