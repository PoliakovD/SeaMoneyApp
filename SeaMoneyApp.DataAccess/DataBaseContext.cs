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