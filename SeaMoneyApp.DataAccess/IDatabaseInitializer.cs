namespace SeaMoneyApp.DataAccess;

public interface IDatabaseInitializer
{
    void Initialize(string dbPath);
}