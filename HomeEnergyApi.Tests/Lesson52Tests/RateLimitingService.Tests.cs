using HomeEnergyApi.Services;
using HomeEnergyApi.Wrapper;

public class RateLimitingServiceTests
{
    private DateTime currentDateTime;
    private RateLimitingService rateLimitingService;
    private StubDateTimeWrapper stubDateTimeWrapper;

    public RateLimitingServiceTests()
    {
        currentDateTime = DateTime.UtcNow;
        stubDateTimeWrapper = new StubDateTimeWrapper(currentDateTime);
        rateLimitingService = new(stubDateTimeWrapper);
    }

    [Fact]
    public void ShouldReturnTrueWhenItIsWeekend()
    {
        var initialTime = DateTime.Parse("2000-01-01 01:01:01");
        stubDateTimeWrapper.SetUp(initialTime); // Saturday
        var result = rateLimitingService.IsWeekend();
        Assert.True(result);
    }

    [Fact]
    public void ShouldReturnFalseWhenItIsWeekday()
    {
        var initialTime = DateTime.Parse("2000-01-03 01:01:01");
        stubDateTimeWrapper.SetUp(initialTime); // Saturday
        var result = rateLimitingService.IsWeekend();
        Assert.False(result);
    }
}

class StubDateTimeWrapper : IDateTimeWrapper
{
    private DateTime dateTime;

    public StubDateTimeWrapper(DateTime dateTime)
    {
        this.dateTime = dateTime;
    }

    public void SetUp(DateTime dateTime)
    {
        this.dateTime = dateTime;
    }
    public DateTime UtcNow()
    {
        return dateTime;
    }
}