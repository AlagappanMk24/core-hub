using Core_API.Application.Contracts.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Core_API.Infrastructure.Services
{
    public class ExchangeRateService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<ExchangeRateService> logger,
        ICacheService cacheService) : IExchangeRateService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<ExchangeRateService> _logger = logger;
        private readonly string _apiKey = configuration["ExchangeRateApi:ApiKey"];
        private readonly string _baseUrl = configuration["ExchangeRateApi:BaseUrl"] ?? "https://v6.exchangerate-api.com/v6/";
        private readonly string _apiProvider = configuration["ExchangeRateApi:Provider"] ?? "exchangerate-api-v6";
        private readonly ICacheService _cacheService = cacheService;
        public async Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency)
        {
            if (fromCurrency == toCurrency)
                return 1;

            var cacheKey = $"exchange_rate_{fromCurrency}_{toCurrency}";

            // Try to get from cache first (cache for 1 hour)
            var cachedRate = await _cacheService.GetAsync<decimal>(cacheKey);
            if (cachedRate != 0)
                return cachedRate;

            try
            {
                decimal rate = 0;

                // Handle different API providers
                rate = _apiProvider.ToLower() switch
                {
                    "exchangerate-api-v6" => await GetExchangeRateFromExchangeRateApiV6Async(fromCurrency, toCurrency),
                    "exchangerate-api" => await GetExchangeRateFromExchangeRateApiV4Async(fromCurrency, toCurrency),
                    _ => await GetExchangeRateFromExchangeRateApiV6Async(fromCurrency, toCurrency),
                };

                if (rate > 0)
                {
                    // Cache for 1 hour
                    await _cacheService.SetAsync(cacheKey, rate, TimeSpan.FromHours(1));
                    return rate;
                }

                _logger.LogWarning("Exchange rate not found from {FromCurrency} to {ToCurrency}", fromCurrency, toCurrency);
                return 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching exchange rate from {FromCurrency} to {ToCurrency}", fromCurrency, toCurrency);
                return 1; // Fallback to 1 on error
            }
        }
        private async Task<decimal> GetExchangeRateFromExchangeRateApiV6Async(string fromCurrency, string toCurrency)
        {
            // ExchangeRate-API v6 format: https://v6.exchangerate-api.com/v6/YOUR-API-KEY/latest/USD
            // Then we need to calculate the rate from base currency to target currency
            // Rate from USD to EUR = (1 / USD rate) * EUR rate? Actually we need to get rates with fromCurrency as base

            var url = $"{_baseUrl}{_apiKey}/latest/{fromCurrency}";

            _logger.LogDebug("Calling ExchangeRate-API v6: {Url}", url);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var exchangeRateResponse = JsonSerializer.Deserialize<ExchangeRateApiV6Response>(content);

            if (exchangeRateResponse?.Result == "success" &&
                exchangeRateResponse.ConversionRates != null &&
                exchangeRateResponse.ConversionRates.TryGetValue(toCurrency, out var rate))
            {
                _logger.LogInformation("Fetched exchange rate from {FromCurrency} to {ToCurrency}: {Rate}",
                    fromCurrency, toCurrency, rate);
                return rate;
            }

            _logger.LogWarning("Failed to get exchange rate from {FromCurrency} to {ToCurrency}", fromCurrency, toCurrency);
            return 0;
        }
        private async Task<decimal> GetExchangeRateFromExchangeRateApiV4Async(string fromCurrency, string toCurrency)
        {
            // ExchangeRate-API v4 format: https://api.exchangerate-api.com/v4/latest/USD?api_key=YOUR_API_KEY
            var url = string.IsNullOrEmpty(_apiKey)
                ? $"{_baseUrl}{fromCurrency}"
                : $"{_baseUrl}{fromCurrency}?api_key={_apiKey}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var rates = JsonSerializer.Deserialize<ExchangeRateApiV4Response>(content);

            if (rates?.Rates != null && rates.Rates.TryGetValue(toCurrency, out var rate))
            {
                return rate;
            }

            return 0;
        }
    }
}

// Response model for ExchangeRate-API v6
public class ExchangeRateApiV6Response
{
    [JsonPropertyName("result")]
    public string Result { get; set; }

    [JsonPropertyName("documentation")]
    public string Documentation { get; set; }

    [JsonPropertyName("terms_of_use")]
    public string TermsOfUse { get; set; }

    [JsonPropertyName("time_last_update_unix")]
    public long TimeLastUpdateUnix { get; set; }

    [JsonPropertyName("time_last_update_utc")]
    public string TimeLastUpdateUtc { get; set; }

    [JsonPropertyName("time_next_update_unix")]
    public long TimeNextUpdateUnix { get; set; }

    [JsonPropertyName("time_next_update_utc")]
    public string TimeNextUpdateUtc { get; set; }

    [JsonPropertyName("base_code")]
    public string BaseCode { get; set; }

    [JsonPropertyName("conversion_rates")]
    public Dictionary<string, decimal> ConversionRates { get; set; }
}

// Response model for ExchangeRate-API v4
public class ExchangeRateApiV4Response
{
    [JsonPropertyName("base")]
    public string Base { get; set; }

    [JsonPropertyName("date")]
    public string Date { get; set; } // API often returns YYYY-MM-DD as string

    [JsonPropertyName("rates")]
    public Dictionary<string, decimal> Rates { get; set; }
}