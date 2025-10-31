using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;

namespace HomeEnergyApi.Services
{
    public class ZipCodeLocationService
    {
        private HttpClient httpClient;
        private readonly IMemoryCache cache;
        private readonly ILogger<ZipCodeLocationService> logger;
        private const string CacheKey = "CurrentWeathCachedZipCodeLocationerForecast";

        public ZipCodeLocationService(HttpClient httpClient, IMemoryCache cache, ILogger<ZipCodeLocationService> logger)
        {
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("HomeEnergyApi/1.0");
            this.httpClient = httpClient;
            this.cache = cache;
            this.logger = logger;
        }

        public async Task<Place> Report(int zipCode)
        {
            if (cache.TryGetValue(CacheKey, out Place cachedPlace))
            {
                logger.LogInformation($"Returning place from cache for {zipCode}");
                return cachedPlace!;
            }

            logger.LogInformation($"Fetching place from api for {zipCode}");
            var response = await httpClient.GetFromJsonAsync<ZipLocationResponse>($"https://api.zippopotam.us/us/{zipCode}");

            var stateFromFluentLinqParallel = response?.Places
                .Where(p => p.State != null)
                .AsParallel()
                .Select(p => p.State)
                .ToList();
            
            logger.LogInformation($"State from API: {stateFromFluentLinqParallel[0]}");

            Place place = response?.Places[0];

            cache.Set(CacheKey, place, TimeSpan.FromSeconds(15));
            return place;
        }
    }
}

public class ZipLocationResponse
{
    [JsonPropertyName("post code")]
    public required string PostCode { get; set; }
    public required string Country { get; set; }
    public required List<Place> Places { get; set; }
}

public class Place
{
    [JsonPropertyName("place name")]
    public required string placename { get; set; }
    public required string State { get; set; }
}