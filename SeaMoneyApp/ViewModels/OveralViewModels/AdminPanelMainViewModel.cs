using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ReactiveUI;
using SeaMoneyApp.DataAccess;
using SeaMoneyApp.DataAccess.Models;
using Splat;

namespace SeaMoneyApp.ViewModels.OveralViewModels;

public class AdminPanelMainViewModel : RoutableViewModel
{
    public Dictionary<string, IEnumerable> Data { get; } = new();

    private IEnumerable _selectedData;

    public IEnumerable SelectedData
    {
        get => _selectedData;
        set => this.RaiseAndSetIfChanged(ref _selectedData, value);
    }

    private string _selectedList;

    public string SelectedList
    {
        get => _selectedList;
        set => this.RaiseAndSetIfChanged(ref _selectedList, value);
    }

    public ObservableCollection<Account> Accounts { get; set; } = [];
    public ObservableCollection<ChangeRubToDollar> ChangeRubToDollars { get; set; } = [];
    public ObservableCollection<Contract> Contracts { get; set; } = [];
    public ObservableCollection<PersonalBonus> PersonalBonuses { get; set; } = [];
    public ObservableCollection<Position> Positions { get; set; } = [];
    public ObservableCollection<Salary> Salaries { get; set; } = [];
    public ObservableCollection<WageLog> WageLogs { get; set; } = [];


    public AdminPanelMainViewModel()
    {
        SelectedList = nameof(Account);
        InitAllTables();
        this.WhenAnyValue(
                x => x.SelectedList,
                x => x.Data,
                (selectedName, allNames) => allNames[selectedName])
            .Subscribe(s => SelectedData = s);
    }

    private void InitAllTables()
    {
        var db = Locator.Current.GetService<DataBaseContext>();

        foreach (var account in db.Accounts.ToList())
        {
            Accounts.Add(account);
        }

        Data.TryAdd(nameof(Account), Accounts);

        foreach (var contract in db.Contracts.AsEnumerable())
        {
            Contracts.Add(contract);
        }

        Data.TryAdd(nameof(Contract), Contracts);

        foreach (var personal in db.PersonalBonuses.AsEnumerable())
        {
            PersonalBonuses.Add(personal);
        }

        Data.TryAdd(nameof(PersonalBonus), PersonalBonuses);

        foreach (var position in db.Positions.AsEnumerable())
        {
            Positions.Add(position);
        }

        Data.TryAdd(nameof(Position), Positions);

        foreach (var salary in db.Salaries.AsEnumerable())
        {
            Salaries.Add(salary);
        }

        Data.TryAdd(nameof(Salary), Salaries);

        foreach (var wageLog in db.WageLogs.AsEnumerable())
        {
            WageLogs.Add(wageLog);
        }

        Data.TryAdd(nameof(WageLog), WageLogs);

        foreach (var change in db.ChangeRubToDollars.AsEnumerable())
        {
            ChangeRubToDollars.Add(change);
        }

        Data.TryAdd(nameof(ChangeRubToDollar), ChangeRubToDollars);
    }
}