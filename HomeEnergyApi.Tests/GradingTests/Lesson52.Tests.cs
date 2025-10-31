using System.Reflection;

public class LessonTests
{
    private static string Lesson52FilePath = @"../../../Lesson52Tests/RateLimitingService.Tests.cs";
    private string Lesson52Content = File.ReadAllText(Lesson52FilePath);
    private readonly string[] _requiredTestMethods =
    {
        "ShouldReturnTrueWhenItIsWeekend",
        "ShouldReturnFalseWhenItIsWeekday"
    };

    [Fact]
    public void Lesson50MathTestsShouldPass()
    {
        var testAssembly = Assembly.GetExecutingAssembly();
        var RateLimitingServiceTests = testAssembly.GetTypes()
            .FirstOrDefault(t => t.Name == "RateLimitingServiceTests");

        Assert.NotNull(RateLimitingServiceTests);

        var instance = Activator.CreateInstance(RateLimitingServiceTests);

        foreach (var requiredMethodName in _requiredTestMethods)
        {
            var testMethod = RateLimitingServiceTests.GetMethod(requiredMethodName);
            Assert.True(testMethod != null, $"Method {requiredMethodName} not found in RateLimitingServiceTests class");

            try
            {
                testMethod.Invoke(instance, null);
            }
            catch (TargetInvocationException ex)
            {
                Assert.Fail($"Test {requiredMethodName} failed: {ex.InnerException?.Message ?? ex.Message}");
            }
        }
    }

    [Fact]
    public void Lesson52ShouldUseMockDateTimeWrapperAndNotStubInTests()
    {
        Assert.Contains("Mock<IDateTimeWrapper>", Lesson52Content);
        Assert.DoesNotContain("StubDateTimeWrapper", Lesson52Content);
    }
}