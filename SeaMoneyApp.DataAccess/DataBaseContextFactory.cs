using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Splat;

namespace SeaMoneyApp.DataAccess;

public class DataBaseContextFactory : IDesignTimeDbContextFactory<DataBaseContext>
{
    private const string DbFileName = "sea_money_app.db";
    private static IDatabaseInitializer? _initializer;

    public static void SetInitializer(IDatabaseInitializer? initializer)
    {
        _initializer = initializer;
    }

    private string GetConnectionString(string connectionName = "DefaultConnection")
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "SeaMoneyApp.DataAccess.appsettings.json";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
            throw new FileNotFoundException($"Embedded resource '{resourceName}' not found.");

        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();

        var config = new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(json)))
            .Build();

        var connectionStringTemplate = config.GetConnectionString(connectionName);
        var basePath = GetBasePath();
        var dbPath = Path.Combine(basePath, DbFileName);

        // Инициализируем БД через внешний инициализатор
        InitializeDatabase(dbPath);

        return connectionStringTemplate.Replace("{BasePath}", basePath);
    }

    private void InitializeDatabase(string dbPath)
    {
        if (File.Exists(dbPath))
            return;

        var directory = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        // Используем внешний инициализатор (на Android) или копируем с диска
        _initializer?.Initialize(dbPath);

        // Если инициализатор не задан — пытаемся скопировать с диска (Desktop)
        if (!File.Exists(dbPath))
        {
            var sourcePath = Path.Combine(AppContext.BaseDirectory, DbFileName);
            if (File.Exists(sourcePath))
            {
                File.Copy(sourcePath, dbPath);
            }
            else
            {
                // Создаём пустую БД
                var options = new DbContextOptionsBuilder<DataBaseContext>()
                    .UseSqlite($"Data Source={dbPath}")
                    .Options;
                var context = new DataBaseContext(options);
                context.Database.EnsureCreated();
            }
        }
    }

    private string GetBasePath()
    {
        string basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        // Debug
        LogHost.Default.Debug("base path for this application is: " + basePath);
        return basePath;
    }

    public DataBaseContext CreateDbContext(string[] args)
    {
        var connectionName = args.Length > 0 ? args[0] : "DefaultConnection";
        var connectionString = GetConnectionString(connectionName);

        var optionsBuilder = new DbContextOptionsBuilder<DataBaseContext>();
        optionsBuilder.UseSqlite(connectionString);

        return new DataBaseContext(optionsBuilder.Options);
    }

    public static DataBaseContext CreateWithDefaultConnectionString() => 
        new DataBaseContextFactory().CreateDbContext(["DefaultConnection"]);
}