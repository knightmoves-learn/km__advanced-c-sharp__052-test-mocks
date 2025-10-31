using HomeEnergyApi.Services;
using HomeEnergyApi.Wrapper;
using Moq;

public class RateLimitingServiceTests
{
    private DateTime currentDateTime;
    private RateLimitingService rateLimitingService;
    private Mock<IDateTimeWrapper> mockDateTimeWrapper;

    public RateLimitingServiceTests()
    {
        currentDateTime = DateTime.UtcNow;
        mockDateTimeWrapper = new Mock<IDateTimeWrapper>();
        rateLimitingService = new(mockDateTimeWrapper.Object);
    }

    [Fact]
    public void ShouldReturnTrueWhenItIsWeekend()
    {
        var initialTime = DateTime.Parse("2000-01-01 01:01:01");
        mockDateTimeWrapper.Setup(d => d.UtcNow()).Returns(initialTime); // Saturday
        var result = rateLimitingService.IsWeekend();
        Assert.True(result);

        mockDateTimeWrapper.Verify(d => d.UtcNow());
    }

    [Fact]
    public void ShouldReturnFalseWhenItIsWeekday()
    {
        var initialTime = DateTime.Parse("2000-01-03 01:01:01");
        mockDateTimeWrapper.Setup(d => d.UtcNow()).Returns(initialTime); // Saturday
        var result = rateLimitingService.IsWeekend();
        Assert.False(result);

        mockDateTimeWrapper.Verify(d => d.UtcNow());
    }
}