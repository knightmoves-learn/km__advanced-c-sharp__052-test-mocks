# 052 Test Mocks
## Lecture

[![# Test Mocks](https://img.youtube.com/vi/GQ0tlIFdSM4/0.jpg)](https://www.youtube.com/watch?v=GQ0tlIFdSM4)

## Instructions

In this lesson you will be testing the method contained in `HomeEnergyApi/Services/RateLimitingService.cs` where a new method `IsWeekend()` has been added. You will need to create test stubs to test this method, in addition to adding tests to `HomeEnergyApi.Tests/Lesson51Tests/RateLimitingService.Tests.cs`. You should NOT change any test files inside of the `HomeEnergyApi.Tests/GradingTests`, these are used to grade your assignment.

- In `HomeEnergyApi/Wrapper/IDateTimeWrapper.cs`
    - Create a new public interface `IDateTimeWrapper` with an interface method `UtcNow()` returning type `DateTime`
- In `HomeEnergyApi/Wrapper/DateTimeWrapper`
    - Create a new public class `DateTimeWrapper` that implements the interface `IDateTimeWrapper`
        - Implement the public method `UtcNow()` by returning `DateTime.UtcNow`
- In `HomeEnergyApi/Program.cs`
    - To the builder's services add a singleton passing the types `IDateTimeWrapper` and `DateTimeWrapper`
- In `HomeEnergyApi/Services/RateLimitingService.cs`
    - Add a new private property `dateTime` of type `IDateTimeWrapper` and add it to the constructor
    - Modify all instances of `DateTime.UtcNow` in the class to use `dateTime.UtcNow()`
- In `HomeEnergyApi.Tests/Lesson51Tests/RateLimitingService.Tests.cs`
    - Create a new class `StubDateTimeWrapper` implementing `IDateTimeWrapper`
        - Add a private property `dateTime` of type `DateTime`
        - Create a constructor where a `DateTime` is passed in and assigned to `dateTime`
        - Create a public method `SetUp` taking one argument of type `DateTime` with no return type
            - Set the value of the newly created property `dateTime` to the passed `DateTime`
        - Create a public method `UtcNow` with the return type `DateTime`
            - Return the newly created property `dateTime`
    - Add the following private properties / types to `RateLimitingServiceTests` 
        - currentDateTime / `currentDateTime`
            - Assign in the constructor to `DateTime.UtcNow`
        - stubDateTime / `StubDateTimeWrapper`
            - Assing in the constructor to a new `StubDateTimeWrapper` with `currentDateTime` passed
        - rateLimitingService / `RateLimitingService?`
            - Assign in the constructor to a new `RateLimitingService` with `stubDateTimeWrapper` passed
    - Create a new unit test `ShouldReturnTrueWhenItIsTheWeekend()` with a `[Fact]` attribute
        - Create a variable to hold the stubbed time by using `DateTime.Parse` and passing a weekend in `string` format
            - You may use `"2000-01-01 01:01:01"` which is a Saturday
        - Call `Setup()` on the property `stubDateTimeWrapper` and pass it your stubbed `DateTime` variable
        - Create a result variable by calling `IsWeekend()` on the property `rateLimitingService`
        - Assert that the value of your result variable is `true`
    - Create a new unit test `ShouldReturnFalseWhenItIsWeekday()` with a `[Fact]` attribute
        - Create a variable to hold the stubbed time by using `DateTime.Parse` and passing a week day in `string` format
            - You may use `"2000-01-03 01:01:01"` which is a Monday
        - Call `Setup()` on the property `stubDateTimeWrapper` and pass it your stubbed `DateTime` variable
        - Create a result variable by calling `IsWeekend()` on the property `rateLimitingService`
        - Assert that the value of your result variable is `false`

## Additional Information

- Some Models may have changed for this lesson from the last, as always all code in the lesson repository is available to view
- Along with `using` statements being added, any packages needed for the assignment have been pre-installed for you, however in the future you may need to add these yourself

## Building toward CSTA Standards:

## Resources
- https://learn.microsoft.com/en-us/dotnet/csharp/advanced-topics/reflection-and-attributes/

Copyright &copy; 2025 Knight Moves. All Rights Reserved.
