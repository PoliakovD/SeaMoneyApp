using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SeaMoneyApp.DataAccess;

public class DataBaseContextFactory : IDesignTimeDbContextFactory<DataBaseContext>
{
    private string GetConnectionString(string connectionName = "DefaultConnection")
    {
        var connectionString = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build()
            .GetConnectionString(connectionName);
        
        return connectionString ?? throw new InvalidOperationException("Connection string is null");
    }
    
    public DataBaseContext CreateDbContext(string[] args)
    {
        var connectionString = args.Length == 0 
            ? GetConnectionString() 
            : GetConnectionString(args[0]);
        
        var optionsBuilder = new DbContextOptionsBuilder<DataBaseContext>();
        optionsBuilder.UseSqlite(connectionString);
        
        return new DataBaseContext(optionsBuilder.Options);
    }

    public static DataBaseContext CreateForTesting() => 
        new DataBaseContextFactory().CreateDbContext(["Test"]);
}