using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SeaMoneyApp.DataAccess;

public class DataBaseContextFactory : IDesignTimeDbContextFactory<DataBaseContext>
{
    const string defaultDbName = "sea_money_app.db";
    private string GetConnectionString(string connectionName = "DefaultConnection")
    {
        // Загружаем appsettings.json как встроенный ресурс
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "SeaMoneyApp.DataAccess.appsettings.json";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
            throw new FileNotFoundException($"Embedded resource '{resourceName}' not found. Check the name and that it's set as 'EmbeddedResource' in .csproj.");

        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();
        
        var config = new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(json)))
            .Build();

        var connectionStringTemplate = config.GetConnectionString(connectionName);

        // Заменяем {BasePath} на реальный путь
        var basePath = GetBasePath();
        
        var connectionString = connectionStringTemplate.Replace("{BasePath}", basePath);
        
        var dbPath = Path.Combine(basePath, defaultDbName);
        CopyEmbeddedDatabase(dbPath);
        return connectionString;
       
    }
    
    private void CopyEmbeddedDatabase(string dbPath)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "SeaMoneyApp.DataAccess.sea_money_app.db";

        using var resourceStream = assembly.GetManifestResourceStream(resourceName);
        if (resourceStream == null)
            throw new InvalidOperationException("Database embedded resource not found.");

        using var fileStream = File.Create(dbPath);
        resourceStream.CopyTo(fileStream);
    }
    
    private string GetBasePath()
    {
        if (OperatingSystem.IsAndroid())
        {
            // На Android используем внутреннюю папку приложения
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return appData; // Например: /data/data/com.company.seamoneyapp/files
        }
        else if (OperatingSystem.IsWindows())
        {
            var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return exePath ?? Directory.GetCurrentDirectory();
            
        }
        else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(folder, "seamoneyapp");
            Directory.CreateDirectory(appFolder);
            return appFolder;
        }
        else
        {
            // Fallback
            var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return exePath ?? Directory.GetCurrentDirectory();
        }
    }
    
    public DataBaseContext CreateDbContext(string[] args)
    {
        var connectionString = args.Length == 0 
            ? GetConnectionString() 
            : GetConnectionString(args[0]);
        
        var optionsBuilder = new DbContextOptionsBuilder<DataBaseContext>();
        optionsBuilder.UseSqlite(connectionString);
        var resultDbContext = new DataBaseContext(optionsBuilder.Options);
        return resultDbContext;
    }

    public static DataBaseContext CreateForTesting() => 
        new DataBaseContextFactory().CreateDbContext(["Test"]);
    public static DataBaseContext CreateWithDefaultConnectionString() => 
        new DataBaseContextFactory().CreateDbContext(["DefaultConnection"]);
    
    
}