using Microsoft.EntityFrameworkCore;
using SeaMoneyApp.DataAccess;
using SeaMoneyApp.DataAccess.Models;
using Xunit;

namespace DataAccessTest;

public class TestIsolatedDbTest : IDisposable
{
    public DataBaseContext Context { get; private set; }

    public TestIsolatedDbTest()
    {
        Context = DataBaseContextFactory.CreateWithTestConnectionString();
        Context.Database.EnsureDeleted();
        Context.Database.Migrate();
    }

    public void Dispose() => Context?.Dispose();

    [Fact]
    public void Test1_AddPosition()
    {
        Context.Positions.Add(new Position { Name = "Master" });
        Context.SaveChanges();

        Assert.Single(Context.Positions.ToList());
    }

    [Fact]
    public void Test2_EmptyAtStart()
    {
        // Каждый тест начинается с чистой БД
        Assert.Empty(Context.Positions.ToList());
    }
}