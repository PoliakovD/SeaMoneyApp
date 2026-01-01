using Microsoft.EntityFrameworkCore;
using SeaMoneyApp.DataAccess.Models;

namespace SeaMoneyApp.DataAccess;

public class DataBaseContext : DbContext
{
    public DbSet<Account> Accounts { get; set; }
    public DbSet<ChangeRubToDollar> ChangeRubToDollars { get; set; }
    public DbSet<PersonalBonus> PersonalBonuses { get; set; }
    public DbSet<Position> Positions { get; set; }
    public DbSet<Salary> Salaries { get; set; }
    public DbSet<WageLog> WageLogs { get; set; }


    public DataBaseContext(DbContextOptions<DataBaseContext> options)
        : base(options)
    {
        
    }

    public void UpdateChangeRubToDollar(ChangeRubToDollar oldCource, ChangeRubToDollar newCource)
    {
        var findedCource = ChangeRubToDollars.FirstOrDefault(c => c.Date == oldCource.Date);
        findedCource.Value = newCource.Value;
        ChangeRubToDollars.Update(findedCource);
        this.SaveChanges();
    }
    
    public void DeleteChangeRubToDollar(ChangeRubToDollar cource)
    {
        var findedCource = ChangeRubToDollars.FirstOrDefault(c => c.Date == cource.Date);
        ChangeRubToDollars.Remove(findedCource);
        this.SaveChanges();
    }
    
    public void AddChangeRubToDollar(ChangeRubToDollar cource)
    {
        ChangeRubToDollars.Add(cource);
        this.SaveChanges();
    }
    public IEnumerable<Position> GetAllPositions()
    {
        return Positions.AsEnumerable();
    }
    
    public IEnumerable<ChangeRubToDollar> GetAllCources()
    {
        return ChangeRubToDollars.AsEnumerable();
    }

    public IEnumerable<Position> GetPositionsByName(string name)
    {
        var positions = GetAllPositions();
        return positions.Where(product => product.Name.Contains(name, StringComparison.CurrentCultureIgnoreCase));
    }
}