
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SeaMoneyApp.DataAccess;

public class DataBaseContextFactory : IDesignTimeDbContextFactory<DataBaseContext>
{
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
        var connectionString = connectionStringTemplate.Replace("{BasePath}", basePath);

        // Сохраняем строку подключения для дальнейшего использования (например, копирование)
        EnsureDatabaseInitialized(connectionString);

        return connectionString;
    }

    private void EnsureDatabaseInitialized(string connectionString)
    {
        var csBuilder = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder(connectionString);
        var dbPath = csBuilder.DataSource; // Полный путь к файлу БД

        if (File.Exists(dbPath))
            return;

        var directory = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        // Используем инициализатор (Android) или копируем с диска
        _initializer?.Initialize(dbPath);

        // Если БД так и не появилась — создаём пустую
        if (!File.Exists(dbPath))
        {
            var options = new DbContextOptionsBuilder<DataBaseContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;
            var context = new DataBaseContext(options);
            context.Database.EnsureCreated();
        }
    }

    private string GetBasePath()
    {
        string basePath;

        if (OperatingSystem.IsAndroid())
        {
            basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }
        else if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            basePath = AppContext.BaseDirectory;
        }
        else
        {
            basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }

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

    public static DataBaseContext CreateWithTestConnectionString() => 
        new DataBaseContextFactory().CreateDbContext(["Test"]);

    public static void SetInitializer(IDatabaseInitializer? initializer)
    {
        _initializer = initializer;
    }

    private static IDatabaseInitializer? _initializer;
}