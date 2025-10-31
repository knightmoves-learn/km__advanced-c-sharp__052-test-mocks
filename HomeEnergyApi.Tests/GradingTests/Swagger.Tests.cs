using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using HomeEnergyApi.Dtos;
using HomeEnergyApi.Models;
using HomeEnergyApi.Tests.Extensions;
using Newtonsoft.Json.Linq;


[TestCaseOrderer("HomeEnergyApi.Tests.Extensions.PriorityOrderer", "HomeEnergyApi.Tests")]
public class SwaggerTests
    : IClassFixture<WebApplicationFactoryDefaultApiKey>
{
    private readonly WebApplicationFactoryDefaultApiKey _factory;
    private JObject swagger1 = new JObject();
    private JObject swagger2 = new JObject();

    public SwaggerTests(WebApplicationFactoryDefaultApiKey factory)
    {
        _factory = factory;
    }

    [Theory, TestPriority(1)]
    [InlineData("/swagger")]
    public async Task HomeEnergyApiIsSuccesfulOnGETRequestToSwaggerPage(string url)
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync(url);

        Assert.True(response.IsSuccessStatusCode,
            $"HomeEnergyApi did not return successful HTTP Response Code on GET request at {url}; instead received {(int)response.StatusCode}: {response.StatusCode}");
    }

    [Fact, TestPriority(2)]
    public async Task SwaggerReturnsCorrectTitlesAndVersionsAtEachEndpoint()
    {
        var client = _factory.CreateClient();

        string result1 = await client.GetStringAsync("/swagger/v1/swagger.json");
        string result2 = await client.GetStringAsync("/swagger/v2/swagger.json");
        swagger1 = JObject.Parse(result1);
        swagger2 = JObject.Parse(result2);

        var info1 = swagger1["info"];
        var info2 = swagger2["info"];

        bool info1Valid = info1?["title"]?.ToString() == "Home Energy Api V1" && info1?["version"]?.ToString() == "v1";
        bool info2Valid = info2?["title"]?.ToString() == "Home Energy Api V2" && info2?["version"]?.ToString() == "v2";
        
        Assert.True(info1Valid, "Swagger did not return a swagger doc with a title of 'Home Energy Api V1' and a version of 'v1' at /swagger/v1/swagger.json");
        Assert.True(info2Valid, "Swagger did not return a swagger doc with a title of 'Home Energy Api V2' and a version of 'v2' at /swagger/v2/swagger.json");

    }

    [Fact, TestPriority(3)]
    public async Task SwaggerReturnsCorrectSecurityDefinitions()
    {
        var client = _factory.CreateClient();

        string result1 = await client.GetStringAsync("/swagger/v1/swagger.json");
        swagger1 = JObject.Parse(result1);

        var apikey = swagger1["components"]?["securitySchemes"]?["ApiKey"];
        var bearer = swagger1["components"]?["securitySchemes"]?["Bearer"];

        bool apiKeyValid = apikey?["type"]?.ToString() == "apiKey" &&
                           apikey?["description"]?.ToString() == "API Key from header" &&
                           apikey?["name"]?.ToString() == "X-Api-Key" &&
                           apikey?["in"]?.ToString() == "header";

        bool bearerValid = bearer?["type"]?.ToString() == "http" &&
                           bearer?["description"]?.ToString() == "JWT Authorization header" &&
                           bearer?["scheme"]?.ToString() == "Bearer" &&
                           bearer?["bearerFormat"]?.ToString() == "JWT";
        
        Assert.True(apiKeyValid && bearerValid, "Swagger did not return a swagger doc with the expected security definitions");
    }
}