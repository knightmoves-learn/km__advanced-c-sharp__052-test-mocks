﻿using System.Text;
using System.Text.Json;
using System.Net;
using Microsoft.Extensions.Logging;
using HomeEnergyApi.Dtos;
using HomeEnergyApi.Models;
using HomeEnergyApi.Pagination;
using HomeEnergyApi.Tests.Extensions;
using System.Runtime.InteropServices;



[TestCaseOrderer("HomeEnergyApi.Tests.Extensions.PriorityOrderer", "HomeEnergyApi.Tests")]
public class ControllersTests
    : IClassFixture<WebApplicationFactoryDefaultApiKey>
{
    private readonly WebApplicationFactoryDefaultApiKey _factory;
    private string _username;
    private string _password;
    public ControllersTests(WebApplicationFactoryDefaultApiKey factory)
    {
        _factory = factory;
        _username = System.Guid.NewGuid().ToString();
        _password = "testPass";
    }

    [Theory, TestPriority(1)]
    [InlineData("/Homes")]
    public async Task HomeEnergyApiReturnsSuccessfulHTTPResponseCodeOnGETHomes(string url)
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync(url);

        Assert.True(response.IsSuccessStatusCode,
            $"HomeEnergyApi did not return successful HTTP Response Code on GET request at {url}; instead received {(int)response.StatusCode}: {response.StatusCode}");
    }

    [Theory, TestPriority(2)]
    [InlineData("admin/UtilityProviders/")]
    public async Task HomeEnergyApiCanPOSTAUtilityProviderGivenAValidUtilityProviderDto(string url)
    {
        var client = _factory.CreateClient();

        UtilityProviderDto postTestUtilityProviderDto = BuildTestUtilProvDto("test energy", new List<string>() { "electric", "natural gas" });
        string strPostTestUtilityProviderDto = JsonSerializer.Serialize(postTestUtilityProviderDto);

        HttpRequestMessage sendRequest = new HttpRequestMessage(HttpMethod.Post, url);
        sendRequest.Content = new StringContent(strPostTestUtilityProviderDto,
                                                Encoding.UTF8,
                                                "application/json");

        var response = await client.SendAsync(sendRequest);
        Assert.True((int)response.StatusCode == 201,
            $"HomeEnergyApi did not return \"201: Created\" HTTP Response Code on POST request at {url}; instead received {(int)response.StatusCode}: {response.StatusCode}");

        string responseContent = await response.Content.ReadAsStringAsync();
        responseContent = responseContent.ToLower();

        bool nameMatch = responseContent.Contains("\"name\":\"test energy\"");
        bool provUtilitiesMatch = responseContent.Contains("\"providedutilities\":[\"electric\",\"natural gas\"]");
        bool hasExpected = nameMatch && provUtilitiesMatch;

        Assert.True(hasExpected,
            $"Home Energy Api did not return the correct UtilityProvider being created on POST at {url}\nHomeDto Sent: {strPostTestUtilityProviderDto}\nHome Received:{responseContent}");
    }

    [Fact, TestPriority(3)]
    public async Task HomeEnergyApiCanRegisterANewUser()
    {
        HttpResponseMessage? newUserResponse = await RegisterUser(_username, _password, "Admin", "123 Test. St");

        Assert.True(newUserResponse?.IsSuccessStatusCode,
            $"HomeEnergyApi did not return successful HTTP Response Code on attempting to register new user at v1/authentication/register; instead received {(int?)newUserResponse?.StatusCode}: {newUserResponse?.StatusCode}");
    }

    [Fact, TestPriority(4)]
    public async Task HomeEnergyApiDoesNotAllowMultipleUsersWithSameUserName()
    {
        await RegisterUser(_username, _password, "Admin", "123 Test. St");
        HttpResponseMessage? newUserResponse = await RegisterUser(_username, _password, "Admin", "123 Test. St");
        string? newUserResponseStr = await newUserResponse.Content.ReadAsStringAsync() ?? "";

        Assert.True(newUserResponseStr == "Username is already taken.",
            $"HomeEnergyApi did not respond with 'Username is already taken'\nReceived:{newUserResponseStr}");
    }

    [Fact, TestPriority(5)]
    public async Task HomeEnergyApiCanProvideABearerToken()
    {
        await RegisterUser(_username, _password, "Admin", "123 Test. St");
        string token = await GetBearerToken(_username, _password, "Admin", "123 Test. St", false);

        bool isValidToken = token.Contains("\"token\":\"");

        Assert.True(isValidToken,
            $"HomeEnergyApi did not return a valid response trying to receive bearer token for a registered user at v1/authentication/token\nReceived:{token}");
    }

    [Theory, TestPriority(6)]
    [InlineData("/admin/Homes")]
    public async Task HomeEnergyApiCanPOSTAHomeGivenAValidHomeDto(string url)
    {
        var client = _factory.CreateClient();
        await RegisterUser($"POST{_username}", _password, "Admin", "123 Test. St");
        string token = await GetBearerToken($"POST{_username}", _password, "Admin", "123 Test. St", true);
        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

        HomeDto postTestHomeDto = BuildTestHomeDto("Test", "123 Test St.", "Test City", 123);
        string strPostTestHomeDto = JsonSerializer.Serialize(postTestHomeDto);

        HttpRequestMessage sendRequest = new HttpRequestMessage(HttpMethod.Post, url);
        sendRequest.Content = new StringContent(strPostTestHomeDto,
                                                Encoding.UTF8,
                                                "application/json");

        var response = await client.SendAsync(sendRequest);

        Assert.True((int)response.StatusCode == 201,
            $"HomeEnergyApi did not return \"201: Created\" HTTP Response Code on POST request at {url}; instead received {(int)response.StatusCode}: {response.StatusCode}");

        string responseContent = await response.Content.ReadAsStringAsync();
        responseContent = responseContent.ToLower();

        bool ownerLastMatch = responseContent.Contains("\"ownerlastname\":\"test\"");
        bool streetAddMatch = responseContent.Contains("\"streetaddress\":\"123 test st.\"");
        bool cityMatch = responseContent.Contains("\"city\":\"test city\"");

        string homeUsageResponse = responseContent.Substring(responseContent.IndexOf("homeusagedata"), responseContent.IndexOf("homeutilityproviders") - responseContent.IndexOf("homeusagedata"));
        bool monthlyUsageMatch = homeUsageResponse.Contains("\"monthlyelectricusage\":123,");

        string expectedHomeId = responseContent[6..responseContent.IndexOf(",")];
        bool homeIdMatch = responseContent.Contains($"\"homeid\":{expectedHomeId}");

        bool hasExpectedHomeData = ownerLastMatch && streetAddMatch && cityMatch && monthlyUsageMatch;

        Assert.True(hasExpectedHomeData,
            $"Home Energy Api did not return the correct Home being created on POST at {url}\nHomeDto Sent: {strPostTestHomeDto}\nHome Received:{responseContent}");

        Assert.True(homeIdMatch,
            $"For the Home created on POST at {url}, the home's id did not match the id within the Home Utility Providers property\nExpected Home Id: {expectedHomeId}\nHome Received:{responseContent}");
    }

    [Theory, TestPriority(7)]
    [InlineData("/admin/Homes")]
    public async Task HomeEnergyApiCanPUTAHomeGivenAValidHomeDto(string url)
    {
        var client = _factory.CreateClient();
        url = url + $"/{await GetIdForPutTest()}";

        HomeDto putTestHomeDto = BuildTestHomeDto("Putty", "123 Put St.", "Put City", 456);

        string strPutTestHomeDto = JsonSerializer.Serialize(putTestHomeDto);

        HttpRequestMessage sendRequest = new HttpRequestMessage(HttpMethod.Put, url);
        sendRequest.Content = new StringContent(strPutTestHomeDto,
                                                Encoding.UTF8,
                                                "application/json");

        var response = await client.SendAsync(sendRequest);
        Assert.True((int)response.StatusCode == 200,
            $"HomeEnergyApi did not return \"200: Ok\" HTTP Response Code on PUT request at {url}; instead received {(int)response.StatusCode}: {response.StatusCode}");

        string responseContent = await response.Content.ReadAsStringAsync();
        responseContent = responseContent.ToLower();

        bool ownerLastMatch = responseContent.Contains("\"ownerlastname\":\"putty\"");
        bool streetAddMatch = responseContent.Contains("\"streetaddress\":\"123 put st.\"");
        bool cityMatch = responseContent.Contains("\"city\":\"put city\"");

        string homeUsageResponse = responseContent.Substring(responseContent.IndexOf("homeusagedata"));
        bool monthlyUsageMatch = homeUsageResponse.Contains("\"monthlyelectricusage\":456,");

        bool hasExpected = ownerLastMatch && streetAddMatch && cityMatch && monthlyUsageMatch;

        Assert.True(hasExpected,
            $"Home Energy Api did not return the correct Home being updated on PUT at {url}\nHomeDto Sent: {strPutTestHomeDto}\nHome Received:{responseContent}");
    }

    [Theory, TestPriority(8)]
    [InlineData("/Homes/Bang")]
    public async Task HomeEnergyApiAppliesGlobalExceptionFilter(string url)
    {
        var client = _factory.CreateClient();
        await RegisterUser($"BANG{_username}", _password, "Admin", "123 Test. St");
        string token = await GetBearerToken($"BANG{_username}", _password, "Admin", "123 Test. St", true);
        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

        var bangResponse = await client.GetAsync(url);
        string bangResponseStr = await bangResponse.Content.ReadAsStringAsync();
        string expected = "{\"message\":\"An unexpected error occurred.\",\"error\":\"You caused a loud bang.\"}";

        Assert.True((int)bangResponse.StatusCode == 500,
            $"HomeEnergyApi did not return '500: Internal Server Error' HTTP Response Code on GET request at {url}; instead received {(int)bangResponse.StatusCode}: {bangResponse.StatusCode}");

        Assert.True(bangResponseStr == expected,
            $"HomeEnergyApi did not return the expected result on GET request at {url}\nExpected:{expected}\nReceived:{bangResponseStr}");
    }

    [Fact, TestPriority(9)]
    public async Task HomeEnergyApiCanRegisterANewUserV2()
    {
        HttpResponseMessage? newUserResponse = await RegisterUserV2(_username, _password, "Admin", "123 Test. St", "Test City", 12345);

        Assert.True(newUserResponse?.IsSuccessStatusCode,
            $"HomeEnergyApi did not return successful HTTP Response Code on attempting to register new user at v2/authentication/register; instead received {(int?)newUserResponse?.StatusCode}: {newUserResponse?.StatusCode}");
    }

    [Fact, TestPriority(10)]
    public async Task HomeEnergyApiCanProvideABearerTokenV2()
    {
        await RegisterUserV2(_username, _password, "Admin", "123 Test. St", "Test City", 12345);
        string token = await GetBearerTokenV2(_username, _password, "Admin", "123 Test. St", "Test City", 12345, false);

        bool isValidToken = token.Contains("\"token\":\"");

        Assert.True(isValidToken,
            $"HomeEnergyApi did not return a valid response trying to receive bearer token for a registered user at v2/authentication/token\nReceived:{token}");
    }

    [Fact, TestPriority(11)]
    public async Task HomeEnergyApiCanUseLogger()
    {
        var client = _factory.CreateClient();
        HttpRequestMessage sendRequest = new HttpRequestMessage(HttpMethod.Post, "v1/authentication/register");
        UserDtoV1 userDto = new();
        userDto.Username = _username;
        userDto.Password = _password;
        userDto.Role = "Admin";
        userDto.HomeStreetAddress = "321 Logging Ave.";
        string userDtoStr = JsonSerializer.Serialize(userDto);

        sendRequest.Content = new StringContent(userDtoStr,
                                        Encoding.UTF8,
                                        "application/json");

        await client.SendAsync(sendRequest);

        var logs = _factory.LoggerProvider.Logs;
        Assert.Contains(logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message.Contains("Encrypted Street Address:"));

        Assert.Contains(logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message.Contains("Hashed Password:"));

        Assert.Contains(logs, log =>
            log.LogLevel == LogLevel.Debug &&
            log.Message.Contains("Saved Username:"));
    }

    [Theory, TestPriority(12)]
    [InlineData("/Homes")]
    public async Task EnsureGetAllHomesIsPaginated(string url)
    {
        var client = _factory.CreateClient();

        // Get the first page of homes
        var response = await client.GetAsync($"{url}?pageNumber=1&pageSize=10");
        //Assert.True(response.IsSuccessStatusCode, $"Failed to get first page of homes: {response.StatusCode}");

        var content = await response.Content.ReadAsStringAsync();
        //dynamic? paginatedResult = JsonSerializer.Deserialize<dynamic>(content);
        bool isPaginated = content.Contains("\"pageNumber\":1") && content.Contains("\"pageSize\":10") && content.Contains("\"totalItems\":");

        Assert.True(isPaginated, $"Expected a paginated response, but the response did not contain pagination information.\nReceived content: \n{content}");
    }

    [Fact, TestPriority(13)]
    public async Task DecryptionLoggingAndAuditServicesLogInformation()
    {
        var client = _factory.CreateClient();
        HttpRequestMessage sendRequest = new HttpRequestMessage(HttpMethod.Post, "v1/authentication/register");
        UserDtoV1 userDto = new();
        userDto.Username = _username;
        userDto.Password = _password;
        userDto.Role = "Admin";
        userDto.HomeStreetAddress = "321 Decryption Ave.";
        string userDtoStr = JsonSerializer.Serialize(userDto);

        sendRequest.Content = new StringContent(userDtoStr,
                                        Encoding.UTF8,
                                        "application/json");

        await client.SendAsync(sendRequest);

        var logs = _factory.LoggerProvider.Logs;
        Assert.Contains(logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message.Contains("[Audit] Decrypted:"));

        Assert.Contains(logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message.Contains("[Logging] Decrypted:"));
    }

    [Theory, TestPriority(14)]
    [InlineData("admin/Homes/Location/50313")]
    public async Task ZipLocationServiceCanReturnNewZipCodeLocation(string url)
    {
        var client = _factory.CreateClient();

        HttpRequestMessage sendRequest = new HttpRequestMessage(HttpMethod.Post, url);
        await client.SendAsync(sendRequest);

        var logs = _factory.LoggerProvider.Logs;
        string logstr = string.Join("\n", logs.Select(log => $"{log.LogLevel}: {log.Message}"));
        Assert.Contains(logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message.Contains("Fetching place from api for"));
        Assert.Contains(logs, log => 
            log.LogLevel == LogLevel.Information &&
            log.Message.Contains("State from API: Iowa"));
    }

    [Theory, TestPriority(15)]
    [InlineData("admin/Homes/Location/50313")]
    public async Task ZipLocationServiceCanReturnFromCacheWithin15Seconds(string url)
    {
        var client = _factory.CreateClient();
        HttpRequestMessage sendRequest = new HttpRequestMessage(HttpMethod.Post, url);
        await client.SendAsync(sendRequest);
        var logs = _factory.LoggerProvider.Logs;

        Assert.Contains(logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message.Contains("Returning place from cache for"));
    }

    public async Task<string> GetBearerToken(string username, string password, string role, string homeStreetAddress, bool trimToken)
    {
        var client = _factory.CreateClient();
        HttpRequestMessage sendRequest = new HttpRequestMessage(HttpMethod.Post, "v1/authentication/token");
        UserDtoV1 userDto = new();
        userDto.Username = username;
        userDto.Password = password;
        userDto.Role = role;
        userDto.HomeStreetAddress = homeStreetAddress;
        string userDtoStr = JsonSerializer.Serialize(userDto);

        sendRequest.Content = new StringContent(userDtoStr,
                                        Encoding.UTF8,
                                        "application/json");

        var response = await client.SendAsync(sendRequest);
        var responseStr = await response.Content.ReadAsStringAsync();

        if (trimToken)
            return responseStr.Trim(new char[] { '{', '}', '"' }).Substring(8);
        else
            return responseStr;
    }

    public async Task<string> GetBearerTokenV2(string username, string password, string role, string streetAddress, string city, int zipCode, bool trimToken)
    {
        var client = _factory.CreateClient();
        HttpRequestMessage sendRequest = new HttpRequestMessage(HttpMethod.Post, "v2/authentication/token");
        UserDtoV2 userDto = new();
        userDto.Username = username;
        userDto.Password = password;
        userDto.Role = role;
        FullAddress fullAddress = new FullAddress();
        fullAddress.StreetAddress = streetAddress;
        fullAddress.City = city;
        fullAddress.ZipCode = zipCode;
        userDto.Address = fullAddress;
        string userDtoStr = JsonSerializer.Serialize(userDto);

        sendRequest.Content = new StringContent(userDtoStr,
                                        Encoding.UTF8,
                                        "application/json");

        var response = await client.SendAsync(sendRequest);
        var responseStr = await response.Content.ReadAsStringAsync();

        if (trimToken)
            return responseStr.Trim(new char[] { '{', '}', '"' }).Substring(8);
        else
            return responseStr;
    }

    public async Task<HttpResponseMessage?> RegisterUser(string username, string password, string role, string homeStreetAddress)
    {
        var client = _factory.CreateClient();
        HttpRequestMessage sendRequest = new HttpRequestMessage(HttpMethod.Post, "v1/authentication/register");
        UserDtoV1 userDto = new();
        userDto.Username = username;
        userDto.Password = password;
        userDto.Role = role;
        userDto.HomeStreetAddress = homeStreetAddress;
        string userDtoStr = JsonSerializer.Serialize(userDto);

        sendRequest.Content = new StringContent(userDtoStr,
                                        Encoding.UTF8,
                                        "application/json");

        var response = await client.SendAsync(sendRequest);
        return response;
    }

    public async Task<HttpResponseMessage?> RegisterUserV2(string username, string password, string role, string streetAddress, string city, int zipCode)
    {
        var client = _factory.CreateClient();
        HttpRequestMessage sendRequest = new HttpRequestMessage(HttpMethod.Post, "v2/authentication/register");
        UserDtoV2 userDto = new();
        userDto.Username = username;
        userDto.Password = password;
        userDto.Role = role;
        FullAddress fullAddress = new FullAddress();
        fullAddress.StreetAddress = streetAddress;
        fullAddress.City = city;
        fullAddress.ZipCode = zipCode;
        userDto.Address = fullAddress;
        string userDtoStr = JsonSerializer.Serialize(userDto);

        sendRequest.Content = new StringContent(userDtoStr,
                                        Encoding.UTF8,
                                        "application/json");

        var response = await client.SendAsync(sendRequest);
        return response;
    }

    public async Task<string> GetIdForPutTest()
    {
        var client = _factory.CreateClient();

        var getAllResponse = await client.GetAsync("/Homes");
        string getAllResponseStr = await getAllResponse.Content.ReadAsStringAsync();
        dynamic? getAllResponseObj = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(getAllResponseStr);
        return getAllResponseObj?.Items?[getAllResponseObj.TotalItems - 1].Id ?? "";
    }

    public UtilityProviderDto BuildTestUtilProvDto(string name, List<string> provUtils)
    {
        UtilityProviderDto build = new UtilityProviderDto();
        build.Name = name;
        build.ProvidedUtilities = provUtils;
        return build;
    }

    public HomeDto BuildTestHomeDto(string ownerLastName, string streetAddress, string city, int monthlyElectricUsage)
    {
        HomeDto build = new HomeDto();
        build.OwnerLastName = ownerLastName;
        build.StreetAddress = streetAddress;
        build.City = city;
        build.MonthlyElectricUsage = monthlyElectricUsage;
        return build;
    }

}

