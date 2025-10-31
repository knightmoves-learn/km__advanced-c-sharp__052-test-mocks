# 052 Test Mocks
## Lecture

[![# Test Mocks](https://img.youtube.com/vi/GQ0tlIFdSM4/0.jpg)](https://www.youtube.com/watch?v=GQ0tlIFdSM4)

## Instructions

Prior to you beginning this lesson the command `dotnet add package Moq` has been ran in the `HomeEnergyApi.Tests` project. You do not need to run this command to complete the assignment.

- In `HomeEnergyApi.Tests/Lesson51Tests/RateLimitingService.Tests.cs`
    - Create a new private property on `RateLimitingServiceTests` named `mockDateTimeWrapper` of type `Mock<IDateTimeWrapper>`
    - In the constructor for `RateLimitingServiceTests` assign `mockDateTimeWrapper` to a new instance of `Mock<IDateTimeWrapper>` and assign it as an `Object` to the initialization for `rateLimitingService`
    - Modify `ShouldReturnTrueWhenItIsWeekend()`...
        - Replace `stubDateTimeWrapper.SetUp()` with `mockDateTimeWrapper.Setup()` passing the argument in to return the `initialTime` when `IDateTimeWrapper.UtcNow()` is called
        - Verify a mocked `IDateTimeWrapper.UtcNow()` was called
    - Modify `ShouldReturnFalseWhenItIsWeekday()`...
        - Replace `stubDateTimeWrapper.SetUp()` with `mockDateTimeWrapper.Setup()` passing the argument in to return the `initialTime` when `IDateTimeWrapper.UtcNow()` is called
        - Verify a mocked `IDateTimeWrapper.UtcNow()` was called
    - Remove the class `StubDateTimeWrapper` and all other instances to the class in this file

## Additional Information

- Some Models may have changed for this lesson from the last, as always all code in the lesson repository is available to view
- Along with `using` statements being added, any packages needed for the assignment have been pre-installed for you, however in the future you may need to add these yourself

## Building toward CSTA Standards:

## Resources
- https://learn.microsoft.com/en-us/dotnet/csharp/advanced-topics/reflection-and-attributes/

Copyright &copy; 2025 Knight Moves. All Rights Reserved.
