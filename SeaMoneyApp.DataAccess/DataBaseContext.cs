using Microsoft.EntityFrameworkCore;
using SeaMoneyApp.DataAccess.Models;

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


    public DataBaseContext(DbContextOptions<DataBaseContext> options)
        : base(options)
    {
        
    }

    public void UpdateChangeRubToDollar(ChangeRubToDollar oldCourse, ChangeRubToDollar newCourse)
    {
        var findedCourse = ChangeRubToDollars.FirstOrDefault(c => c.Id == oldCourse.Id);
        
        if (findedCourse == null)  throw new ArgumentNullException(nameof(findedCourse));
        
        findedCourse.Value = newCourse.Value;
        findedCourse.Date = newCourse.Date;
        ChangeRubToDollars.Update(findedCourse);
        this.SaveChanges();
    }
    
    public void DeleteChangeRubToDollar(ChangeRubToDollar course)
    {
        var findedCourse = ChangeRubToDollars.FirstOrDefault(c => c.Date == course.Date);
        ChangeRubToDollars.Remove(findedCourse);
        this.SaveChanges();
    }
    
    public void AddChangeRubToDollar(ChangeRubToDollar course)
    {
        ChangeRubToDollars.Add(course);
        this.SaveChanges();
    }
    public void AddContract(Contract contract)
    {
        Contracts.Add(contract);
        this.SaveChanges();
    }
    public void UpdateContract(Contract oldСontract, Contract newContract)
    {
        var findedContract = Contracts.FirstOrDefault(c => c.Id == oldСontract.Id);
        
        if (findedContract == null)  throw new ArgumentNullException(nameof(findedContract));
        
        findedContract.BeginDate=newContract.BeginDate;
        findedContract.EndDate=newContract.EndDate;
        findedContract.VesselName = newContract.VesselName;
        findedContract.ContractDescription = newContract.ContractDescription;
        
        Contracts.Update(findedContract);
        this.SaveChanges();
    }
    public IEnumerable<Position> GetAllPositions()
    {
        return Positions.AsEnumerable();
    }
    
    public List<ChangeRubToDollar> GetAllCources()
    {
        return ChangeRubToDollars.OrderBy(c=>c.Date).ToList();
    }
    
    public IEnumerable<Position> GetPositionsByName(string name)
    {
        var positions = GetAllPositions();
        return positions.Where(product => product.Name.Contains(name, StringComparison.CurrentCultureIgnoreCase));
    }
    public void DeleteContract(Contract contract)
    {
        var findedCourse = Contracts.FirstOrDefault(c => c.Id == contract.Id);
        Contracts.Remove(findedCourse);
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
        await foreach (var item in this.Contracts.Where(x => x.Account.Id == user.Id).AsAsyncEnumerable())
        {
            yield return item;
        }
    }
    
    
}