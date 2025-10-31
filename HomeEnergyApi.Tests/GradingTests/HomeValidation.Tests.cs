using HomeEnergyApi.Dtos;
using HomeEnergyApi.Validations;
using HomeEnergyApi.Controllers;
using HomeEnergyApi.Attributes;
using HomeEnergyApi.Models;
using HomeEnergyApi.Services;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using AutoMapper;

public class HomeValidationTests
{
    private HomeDto targetHomeDto;
    public HomeValidationTests()
    {
        targetHomeDto = new HomeDto();
        targetHomeDto.OwnerLastName = "test";
    }

    [Fact]
    public void HomeDtoShouldBeInvalidWhenAppliedToObjectsOtherThanHomeDto()
    {
        var homeValidAttribute = new HomeStreetAddressValidAttribute();
        var result = homeValidAttribute.GetValidationResult("not a HomeDto", new ValidationContext(targetHomeDto));

        Assert.True(result?.ErrorMessage != null,
            $"When provided with a 'string' rather than a 'HomeDto' , HomeStreetAddressValidAttribute did not return an invalid result");
        Assert.True(result?.ErrorMessage == "Invalid home data.",
            $"HomeStreetAddressValidAttribute did not return the correct error message on an invalid result\nExpected:Invalid home data.\nReceived:{result?.ErrorMessage}");
    }

    [Fact]
    public void HomeDtoShouldBeInvalidWhenStreetAddressDoesNotContainDigit()
    {
        targetHomeDto.StreetAddress = "one two three Test St.";
        var homeValidAttribute = new HomeStreetAddressValidAttribute();
        var result = homeValidAttribute.GetValidationResult(targetHomeDto, new ValidationContext(targetHomeDto));

        Assert.True(result?.ErrorMessage != null,
            $"When provided with StreetAddress : '{targetHomeDto.StreetAddress}' , HomeStreetAddressValidAttribute did not return an invalid result");
        Assert.True(result?.ErrorMessage == "Street Address must contain a number and have fewer than 64 characters",
            $"HomeStreetAddressValidAttribute did not return the correct error message on an invalid result\nExpected:Street Address must contain a number and have fewer than 64 characters\nReceived:{result?.ErrorMessage}");
    }

    [Fact]
    public void HomeDtoShouldBeInvalidWhenStreetAddressIsLongerThan64Characters()
    {
        targetHomeDto.StreetAddress = "123 Test St. 123 Test St. 123 Test St. 123 Test St. 123 Test St. 123 Test St. 123 Test St. 123 Test St. 123 Test St. ";
        var homeValidAttribute = new HomeStreetAddressValidAttribute();
        var result = homeValidAttribute.GetValidationResult(targetHomeDto, new ValidationContext(targetHomeDto));

        Assert.True(result?.ErrorMessage != null,
            $"When provided with StreetAddress : '{targetHomeDto.StreetAddress}' , HomeStreetAddressValidAttribute did not return an invalid result");
        Assert.True(result?.ErrorMessage == "Street Address must contain a number and have fewer than 64 characters",
            $"HomeStreetAddressValidAttribute did not return the correct error message on an invalid result\nExpected:Street Address must contain a number and have fewer than 64 characters\nReceived:{result?.ErrorMessage}");
    }

    [Fact]
    public void HomeDtoShouldBeInvalidWhenStreetAddressIsLongerThan64CharactersAndHasNoDigits()
    {
        targetHomeDto.StreetAddress = "one two three Test St one two three Test St one two three Test St one two three Test St one two three Test St one two three Test St ";
        var homeValidAttribute = new HomeStreetAddressValidAttribute();
        var result = homeValidAttribute.GetValidationResult(targetHomeDto, new ValidationContext(targetHomeDto));

        Assert.True(result?.ErrorMessage != null,
            $"When provided with StreetAddress : '{targetHomeDto.StreetAddress}' , HomeStreetAddressValidAttribute did not return an invalid result");
        Assert.True(result?.ErrorMessage == "Street Address must contain a number and have fewer than 64 characters",
            $"HomeStreetAddressValidAttribute did not return the correct error message on an invalid result\nExpected:Street Address must contain a number and have fewer than 64 characters\nReceived:{result?.ErrorMessage}");
    }

    [Fact]
    public void HomeDtoShouldBeValidWhenStreetAddressHasADigitAndFewerThan64Characters()
    {
        targetHomeDto.StreetAddress = "123 Test St.";
        var homeValidAttribute = new HomeStreetAddressValidAttribute();
        var result = homeValidAttribute.GetValidationResult(targetHomeDto, new ValidationContext(targetHomeDto));

        Assert.True(result == ValidationResult.Success,
            $"When provided with StreetAddress : '{targetHomeDto.StreetAddress}' , HomeStreetAddressValidAttribute did not return a valid result\nReceived Error Message: {result?.ErrorMessage}");
    }

    [Fact]
    public void HomeAdminControllerHasValidateMontlhyElectricAttributeOnCreatingHome()
    {
        var methodInfo = typeof(HomeAdminController).GetMethod(nameof(HomeAdminController.CreateHome));

        var hasFilter = methodInfo?.GetCustomAttributes(typeof(ValidateMonthlyElectricAttribute), inherit: true)
                                  .Any();

        Assert.True(hasFilter);
    }

    [Fact]
    public void ValidateMonthlyElectricAttributeReturnsBadRequestWhenMonthlyElectricUsageIsInvalid()
    {
        var attribute = new ValidateMonthlyElectricAttribute(0, 10000);
        var context = new ActionExecutingContext(
            new ActionContext(
                Mock.Of<HttpContext>(),
                Mock.Of<RouteData>(),
                Mock.Of<ActionDescriptor>()),
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>
            {
                { "homeDto", new HomeDto { MonthlyElectricUsage = -1 } }
            },
        new HomeAdminController(
            Mock.Of<IWriteRepository<int, Home>>(),
            new Mock<ZipCodeLocationService>(
                new HttpClient(),
                Mock.Of<IMemoryCache>(),
                Mock.Of<ILogger<ZipCodeLocationService>>()).Object,
            new Mock<HomeUtilityProviderService>(
                Mock.Of<IReadRepository<int, UtilityProvider>>(),
                Mock.Of<IWriteRepository<int, HomeUtilityProvider>>()).Object,
            Mock.Of<IMapper>()
        ));

        attribute.OnActionExecuting(context);

        Assert.IsType<BadRequestObjectResult>(context.Result);
        var badRequestResult = context.Result as BadRequestObjectResult;
        Assert.NotNull(badRequestResult);
        Assert.Equal("MonthlyElectricUsage must be between 0 and 10000 kwH.", badRequestResult.Value);
    }
}