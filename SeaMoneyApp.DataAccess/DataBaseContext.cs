using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.EntityFrameworkCore;
using SeaMoneyApp.DataAccess.Models;
using Splat;

namespace SeaMoneyApp.DataAccess;

public class DataBaseContext : DbContext
{
    public DbSet<Account> Accounts { get; set; }
    public DbSet<ChangeRubToDollar> ChangeRubToDollars { get; set; }
    public DbSet<PersonalBonus> PersonalBonuses { get; set; }
    public DbSet<Contract> Contracts { get; set; }
    public DbSet<Position> Positions { get; set; }
    public DbSet<Salary> Salaries { get; set; }
    public DbSet<WageLog> WageLogs { get; set; }

    private readonly BehaviorSubject<string?> _errorMessageSubject = new(null);
    public IObservable<string?> WhenErrorMessageChanged => _errorMessageSubject.AsObservable();

    public DataBaseContext(DbContextOptions<DataBaseContext> options)
        : base(options)
    {
    }

    public void UpdateChangeRubToDollar(ChangeRubToDollar oldCourse, ChangeRubToDollar newCourse)
    {
        var findedCourse = ChangeRubToDollars.FirstOrDefault(c => c.Id == oldCourse.Id);

        if (findedCourse == null) throw new ArgumentNullException(nameof(findedCourse));

        findedCourse.Value = newCourse.Value;
        findedCourse.Date = newCourse.Date;
        ChangeRubToDollars.Update(findedCourse);
        this.SaveChanges();
    }

    public void DeleteChangeRubToDollar(ChangeRubToDollar course)
    {
        var findedCourse = ChangeRubToDollars.FirstOrDefault(c => c.Id == course.Id);
        if (findedCourse is null) throw new ArgumentNullException(nameof(findedCourse));
        findedCourse.IsDeleted = true;
        ChangeRubToDollars.Update(findedCourse);
        this.SaveChanges();
    }

    public void AddChangeRubToDollar(ChangeRubToDollar course)
    {
        try
        {
            var findedCourse = ChangeRubToDollars.FirstOrDefault(c => c.Date.Date == course.Date.Date);
            LogHost.Default.Info($"findedCourse: {findedCourse}");
            if (findedCourse == null) ChangeRubToDollars.Add(course);
            else
            {
                findedCourse.Value = course.Value;
                findedCourse.IsDeleted = false;
                ChangeRubToDollars.Update(findedCourse);
            }
        }
        catch (Exception exception)
        {
            LogHost.Default.Error(exception.Message);
            throw;
        }
        
        
        this.SaveChanges();
    }

    public void AddContract(Contract contract)
    {
        
        var findedContract = Contracts.Include(cont=>cont.Account)
            .Where(c=>c.Account.Id==contract.Account.Id)
            .FirstOrDefault(c => c.BeginDate == contract.BeginDate);

        if (findedContract == null) Contracts.Add(contract);
        else
        {
            findedContract.BeginDate = contract.BeginDate;
            findedContract.EndDate = contract.EndDate;
            findedContract.VesselName = contract.VesselName;
            findedContract.ContractDescription = contract.ContractDescription;
            findedContract.Position = contract.Position;
            findedContract.IsDeleted = false;
            Contracts.Update(findedContract);
        }
        this.SaveChanges();
    }

    public void UpdateContract(Contract oldСontract, Contract newContract)
    {
        var findedContract = Contracts.FirstOrDefault(c => c.Id == oldСontract.Id);

        if (findedContract == null) throw new ArgumentNullException(nameof(findedContract));

        findedContract.BeginDate = newContract.BeginDate;
        findedContract.EndDate = newContract.EndDate;
        findedContract.VesselName = newContract.VesselName;
        findedContract.ContractDescription = newContract.ContractDescription;
        findedContract.IsDeleted = false;
        Contracts.Update(findedContract);
        this.SaveChanges();
    }

    public IEnumerable<Position> GetAllPositions()
    {
        return Positions.AsEnumerable();
    }

    public List<ChangeRubToDollar> GetAllCources()
    {
        return ChangeRubToDollars.Where(x=>!x.IsDeleted).OrderBy(c => c.Date).ToList();
    }

    public IEnumerable<Position> GetPositionsByName(string name)
    {
        var positions = GetAllPositions();
        return positions.Where(product => product.Name.Contains(name, StringComparison.CurrentCultureIgnoreCase));
    }

    public void DeleteContract(Contract contract)
    {
        var findedContract = Contracts.FirstOrDefault(c => c.Id == contract.Id);
        findedContract.IsDeleted = true;
        Contracts.Update(findedContract);
        this.SaveChanges();
    }

    public async Task UpdateAccountAsync(Account oldAccount, Account newAccount, CancellationToken token)
    {
        try
        {
            token.ThrowIfCancellationRequested();
            var findedAccount = Accounts.FirstOrDefault(c => c.Id == oldAccount.Id);

            if (findedAccount == null) throw new ArgumentNullException(nameof(findedAccount));

            findedAccount.Login = newAccount.Login;
            findedAccount.Password = newAccount.Password;
            findedAccount.ToursInRank = newAccount.ToursInRank;
            findedAccount.Password = newAccount.Password;
            findedAccount.Position = await Positions.FirstAsync(p => p.Id == newAccount!.Position!.Id, token);

            Accounts.Update(findedAccount);

            await this.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async IAsyncEnumerable<Contract>? GetUserContractsAsyncEnumerable(Account user)
    {
        await foreach (var item in this.Contracts
                           .Where(x => x.Account.Id == user.Id && !x.IsDeleted)
                           .OrderBy(x => x.BeginDate).Reverse()
                           .AsAsyncEnumerable())
        {
            yield return item;
        }
    }

    public async IAsyncEnumerable<WageLog>? GetUserWageLogsAsyncEnumerable(Account user)
    {
        await foreach (var item in this.WageLogs
                           .Include(x => x.ChangeRubToDollar)
                           .Include(x => x.Account)
                           .Include(x => x.Contract)
                           .Include(x => x.Position)
                           .Where(x => x.Account.Id == user.Id)
                           .OrderBy(x => x.Date).Reverse()
                           .AsAsyncEnumerable())
        {
            yield return item;
        }
    }

    public void DeleteWageLog(WageLog wageLog)
    {
        var findedWageLog = WageLogs.FirstOrDefault(c => c.Id == wageLog.Id);
        if (findedWageLog == null) throw new ArgumentNullException(nameof(findedWageLog));
        WageLogs.Remove(findedWageLog);
        this.SaveChanges();
    }

    public async Task<bool> AddWageLogAsync(WageLog wageLog)
    {
        try
        {
            var contract = await Contracts.FirstOrDefaultAsync(c => c.Id == wageLog.Contract.Id);
            if (contract == null)
            {
                var errMsg = $"Контракт не найден в Бд";
                _errorMessageSubject.OnNext(errMsg);
                LogHost.Default.Error(errMsg);
                return false;
            }

            if (wageLog.Date < contract.BeginDate)
            {
                var errMsg =
                    $"Дата лога {wageLog.Date} не может быть раньше начала контракта {contract.BeginDate}";
                _errorMessageSubject.OnNext(errMsg);
                LogHost.Default.Error(errMsg);
                return false;
            }

            wageLog.ChangeRubToDollar = await GetChangeRubToDollarOnWageLogDateAsync(wageLog.Date!.Value);
            if (wageLog.ChangeRubToDollar is null)
            {
                var errMsg = $"В базе данных не найден курс для {wageLog.Date:d}";
                _errorMessageSubject.OnNext(errMsg);
                return false;
            }
           
            var findedWageLog = WageLogs
                                .Include(a => a.Account)
                                .Where(wl => wl.Account.Id == wageLog.Account.Id)
                                .FirstOrDefault(wlog => wlog.Date == wageLog.Date);
            if (findedWageLog == null) await WageLogs.AddAsync(wageLog);
            else
            {
                findedWageLog.Date = wageLog.Date;
                findedWageLog.ChangeRubToDollar = wageLog.ChangeRubToDollar;
                findedWageLog.Contract = wageLog.Contract;
                findedWageLog.IsDeleted = false;

                WageLogs.Update(findedWageLog);
            }

            this.SaveChanges();
            return true;
        }
        catch (Exception ex)
        {
            LogHost.Default.Error(ex.Message);
            throw;
        }
    }

    public async Task<bool> UpdateWageLogAsync(WageLog oldWageLog, WageLog newWageLog,
        CancellationToken token = default)
    {
        try
        {
            var findedWageLog = WageLogs.FirstOrDefault(c => c.Id == oldWageLog.Id);

            if (findedWageLog == null)
            {
                _errorMessageSubject.OnNext($"WageLog dated {oldWageLog.Date} Not Found in DB");
                return false;
            }

            findedWageLog.ChangeRubToDollar = await GetChangeRubToDollarOnWageLogDateAsync(newWageLog.Date!.Value);
            if (findedWageLog.ChangeRubToDollar is null)
            {
                var errMsg = $"В базе данных не найден курс для {findedWageLog.Date:c}";
                _errorMessageSubject.OnNext(errMsg);
                return false;
            }

            if (findedWageLog.Date < findedWageLog.Contract.BeginDate)
            {
                var errMsg =
                    $"Дата лога {findedWageLog.Date} не может быть раньше начала контракта {findedWageLog.Contract.BeginDate}";
                _errorMessageSubject.OnNext(errMsg);
                LogHost.Default.Error(errMsg);
                return false;
            }

            findedWageLog.Date = newWageLog.Date;
            findedWageLog.AmountInRub = newWageLog.AmountInRub;
            findedWageLog.Contract = newWageLog.Contract;
            findedWageLog.ChangeRubToDollar = await GetChangeRubToDollarOnWageLogDateAsync(findedWageLog.Date.Value);
            if (findedWageLog.ChangeRubToDollar is null)
            {
                var errMsg = $"В базе данных не найден курс для {findedWageLog.Date:d}";
                _errorMessageSubject.OnNext(errMsg);
                return false;
            }


            WageLogs.Update(findedWageLog);
            this.SaveChanges();
        }
        catch (Exception e)
        {
            LogHost.Default.Error(e.Message);
            throw;
        }

        return true;
    }

    public async Task<ChangeRubToDollar?> GetChangeRubToDollarOnWageLogDateAsync(DateTime date)
    {
        try
        {
            int defaultDay = 15;
            int year = date.Year;
            int month = date.Month;
            if (date.Day < 15) month--;
            if (month == 0)
            {
                month = 12;
                year--;
            }

            var searchedDate = new DateTime(year, month, defaultDay);
            return await ChangeRubToDollars.FirstOrDefaultAsync(x => x.Date == searchedDate);
        }
        catch (Exception e)
        {
            LogHost.Default.Error(e.Message);
            throw;
        }
    }
}