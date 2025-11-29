using System.IO;
using SeaMoneyApp.DataAccess;

namespace SeaMoneyApp.Android;

public class AndroidDatabaseInitializer : IDatabaseInitializer
{
    public void Initialize(string dbPath)
    {
        try
        {
            using var assetStream = global::Android.App.Application.Context.Assets.Open("sea_money_app.db");
            using var fileStream = File.Create(dbPath);
            assetStream.CopyTo(fileStream);
        }
        catch
        {
            // Если не удалось — ничего не делаем, пусть создаётся пустая
        }
    }
}