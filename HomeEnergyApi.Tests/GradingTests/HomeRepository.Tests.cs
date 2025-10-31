using HomeEnergyApi.Models;
using HomeEnergyApi.Tests.Extensions;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

[TestCaseOrderer("HomeEnergyApi.Tests.Extensions.PriorityOrderer", "HomeEnergyApi.Tests")]
public class HomeRepositoryTests
{
    private Home testHomeToSave = new Home("Savey", "789 Save Ave.", "Savetown");
    private Home testHomeToUpdate = new Home("Updater", "333 Update Ave.", "Updateeee");
    private Home nullHome = null;


    private List<Home> testHomes = new List<Home>()
        {
            new Home("Testy", "456 Assert St.", "Unitville"),
            new Home("Test", "123 Test St.", "Test City")
        };

    private readonly DbConnection _connection;
    private readonly DbContextOptions<HomeDbContext> _contextOptions;

    public HomeRepositoryTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        _contextOptions = new DbContextOptionsBuilder<HomeDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new HomeDbContext(_contextOptions);

        if (context.Database.EnsureCreated())
        {
            using var viewCommand = context.Database.GetDbConnection().CreateCommand();
            viewCommand.CommandText = @"
CREATE VIEW AllResources AS
SELECT *
FROM Homes;";
            viewCommand.ExecuteNonQuery();
        }

        context.AddRange(testHomes);
        context.SaveChanges();
    }

    HomeDbContext CreateContext() => new HomeDbContext(_contextOptions);

    [Fact, TestPriority(1)]
    public void HomeRepositoryCanFindAllHomes()
    {
        using var context = CreateContext();
        var homeRepository = new HomeRepository(context);

        var homes = homeRepository.FindAll();

        Assert.Collection(
            homes,
            h => Assert.True(h.OwnerLastName == testHomes[0].OwnerLastName),
            h => Assert.True(h.OwnerLastName == testHomes[1].OwnerLastName));
    }

    [Fact, TestPriority(2)]
    public void HomeRepositoryCanFindHomeById()
    {
        using var context = CreateContext();
        var homeRepository = new HomeRepository(context);

        var home = homeRepository.FindById(1);

        Assert.True(home.OwnerLastName == testHomes[0].OwnerLastName);
    }

    [Fact, TestPriority(3)]
    public void HomeRepositoryCanFindByOwnerLastName()
    {
        using var context = CreateContext();
        var homeRepository = new HomeRepository(context);
       
        string ownerLastToFind = "Testy";
        var homes = homeRepository.FindByOwnerLastName(ownerLastToFind);

        Assert.True(homes.Count() == 1);
        Assert.True(homes[0].OwnerLastName == ownerLastToFind);
    }

    [Fact, TestPriority(4)]
    public void HomeRepositoryCanSaveANewHome()
    {
        using var context = CreateContext();
        var homeRepository = new HomeRepository(context);

        homeRepository.Save(testHomeToSave);
        var home = homeRepository.FindById(3);

        Assert.True(home.OwnerLastName == testHomeToSave.OwnerLastName);
    }

    [Fact, TestPriority(5)]
    public void HomeRepositoryCanUpdateAHome()
    {
        using var context = CreateContext();
        var homeRepository = new HomeRepository(context);

        homeRepository.Update(1, testHomeToUpdate);
        var home = homeRepository.FindById(1);

        Assert.True(home.OwnerLastName == testHomeToUpdate.OwnerLastName);
    }

    [Fact, TestPriority(6)]
    public void HomeRepositoryCanRemoveById()
    {
        using var context = CreateContext();
        var homeRepository = new HomeRepository(context);

        homeRepository.RemoveById(2);
        var home = homeRepository.FindById(2);

        Assert.True(home == null);
    }

    [Fact, TestPriority(7)]
    public void HomeRepositoryCanReturnCount()
    {
        using var context = CreateContext();
        var homeRepository = new HomeRepository(context);

        var count = homeRepository.Count();

        Assert.True(count == 2);
    }

    [Fact, TestPriority(8)]
    public void HomeRepositoryImplementsIReadRepositoryINTHOME()
    {
        Assert.True(typeof(IReadRepository<int, Home>).IsAssignableFrom(typeof(HomeRepository)),
            "The class HomeRepository Does Not Implement \"IRepository<int, Home>\"");
    }

    [Fact, TestPriority(9)]
    public void HomeRepositoryImplementsIWriteRepositoryINTHOME()
    {
        Assert.True(typeof(IWriteRepository<int, Home>).IsAssignableFrom(typeof(HomeRepository)),
            "The class HomeRepository Does Not Implement \"IRepository<int, Home>\"");
    }

    [Fact, TestPriority(10)]
    public void HomeRepositoryWillNotReturnAnOwnerWhenTheOwnersLastNameIsAnInitial()
    {
        using var context = CreateContext();
        var homeRepository = new HomeRepository(context);
       
        string ownerLastToFind = "T";
        var homes = homeRepository.FindByOwnerLastName(ownerLastToFind);

        Assert.True(homes.Count() == 0);
    }

    [Fact, TestPriority(11)]
    public void HomeRepositoryCanTellIfHomeIsNullOrNot()
    {
        using var context = CreateContext();
        var homeRepository = new HomeRepository(context);

        bool nullResult = homeRepository.IsHomeNull(nullHome);
        bool notNullResult = homeRepository.IsHomeNull(testHomeToSave);

        Assert.True(nullResult, "Home Repository did not return true for null home.");
        Assert.False(notNullResult, "Home Repository did not return false for non-null home.");
    }

    [Fact, TestPriority(12)]
    public void HomeRepositoryCanTellIfHomeIdIsNullOrNot()
    {
        using var context = CreateContext();
        var homeRepository = new HomeRepository(context);

        bool nullResult = homeRepository.IsHomeIdNull(nullHome);
        bool notNullResult = homeRepository.IsHomeIdNull(testHomeToSave);

        Assert.True(nullResult, "Home Repository did not return true for null home Id.");
        Assert.False(notNullResult, "Home Repository did not return false for non-null home Id.");
    }
}
