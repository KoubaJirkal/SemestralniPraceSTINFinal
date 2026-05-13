using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SemestralniPraceSTIN.DTOs;
using SemestralniPraceSTIN.Models;
using System.Text.Json;

namespace SemestralniPraceSTIN.Services
{
    public class ExchangeRateService
    {
        private readonly HttpClient _httpClient;
        private readonly LoggingService _loggingService;
        private readonly AppDbContext _db;
        private readonly IMemoryCache _cache;
        public ExchangeRateService(
            HttpClient httpClient,
            LoggingService loggingService,
            IMemoryCache cache,
            AppDbContext db)
        {
            _httpClient = httpClient;
            _loggingService = loggingService;
            _cache = cache;
            _db = db;
        }

        public virtual async Task<ExchangeRateResponse?> GetLatestRates(
        string baseCurrency,
        string symbols)
        {
            var cacheKey = $"latest_{baseCurrency}_{symbols}";

            try
            {
                //var url = $"sigma";
                var url = $"https://api.frankfurter.dev/v1/latest?base={baseCurrency}&symbols={symbols}";


                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _loggingService.LogError(
                        $"API error: {response.StatusCode}");

                    if (_cache.TryGetValue(cacheKey, out ExchangeRateResponse cachedData))
                    {
                        _loggingService.LogInfo(
                            "Using cached latest rates.");

                        return cachedData;
                    }

                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var result = JsonSerializer.Deserialize<ExchangeRateResponse>(
                    json,
                    options);

                if (result != null)
                {
                    _cache.Set(
                        cacheKey,
                        result,
                        TimeSpan.FromMinutes(30));
                    var oldRates = _db.ExchangeRates
                    .Where(r => r.BaseCurrency == result.Base);

                    _db.ExchangeRates.RemoveRange(oldRates);

                    foreach (var rate in result.Rates)
                    {
                        var exchangeRate = new ExchangeRate
                        {
                            BaseCurrency = result.Base,
                            Currency = rate.Key,
                            Rate = rate.Value,
                            CreatedAt = DateTime.Now
                        };

                        _db.ExchangeRates.Add(exchangeRate);
                    }

                    await _db.SaveChangesAsync();
                }

                return result;
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex.Message);

                if (_cache.TryGetValue(cacheKey, out ExchangeRateResponse cachedData))
                {
                    _loggingService.LogInfo(
                        "Using cached latest rates after exception.");

                    return cachedData;
                }

                var dbRates = await _db.ExchangeRates
                .Where(r => r.BaseCurrency == baseCurrency)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

                if (dbRates.Any())
                {
                    var groupedRates = dbRates
                        .GroupBy(r => r.Currency)
                        .ToDictionary(
                            g => g.Key,
                            g => g.First().Rate);

                    return new ExchangeRateResponse
                    {
                        Base = baseCurrency,
                        Rates = groupedRates
                    };
                }

                return null;
            }
        }

        public virtual async Task<CurrencyResultDto?> GetStrongestCurrency(
    string baseCurrency,
    string symbols)
        {
            var data = await GetLatestRates(baseCurrency, symbols);

            if (data == null || data.Rates.Count == 0)
                return null;

            var strongest = data.Rates
                .OrderByDescending(r => r.Value)
                .First();

            return new CurrencyResultDto
            {
                Currency = strongest.Key,
                Rate = strongest.Value
            };
        }

        public virtual async Task<CurrencyResultDto?> GetWeakestCurrency(
            string baseCurrency,
            string symbols)
        {
            var data = await GetLatestRates(baseCurrency, symbols);

            if (data == null || data.Rates.Count == 0)
                return null;

            var weakest = data.Rates
                .OrderBy(r => r.Value)
                .First();

            return new CurrencyResultDto
            {
                Currency = weakest.Key,
                Rate = weakest.Value
            };
        }

        public virtual async Task<HistoricalRateResponse?> GetHistoricalRates(
    string startDate,
    string endDate,
    string baseCurrency,
    string symbols)
        {
            var cacheKey =
                $"history_{startDate}_{endDate}_{baseCurrency}_{symbols}";

            try
            {
                var url =
                    $"https://api.frankfurter.dev/v1/{startDate}..{endDate}" +
                    $"?base={baseCurrency}&symbols={symbols}";

                var response =
                    await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _loggingService.LogError(
                        $"Historical API error: {response.StatusCode}");

                    if (_cache.TryGetValue(
                        cacheKey,
                        out HistoricalRateResponse cachedData))
                    {
                        _loggingService.LogInfo(
                            "Using cached historical data.");

                        return cachedData;
                    }

                    return null;
                }

                var json =
                    await response.Content.ReadAsStringAsync();

                var options =
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                var result =
                    JsonSerializer.Deserialize<HistoricalRateResponse>(
                        json,
                        options);

                if (result != null)
                {
                    _cache.Set(
                        cacheKey,
                        result,
                        TimeSpan.FromMinutes(30));
                }

                return result;
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex.Message);

                if (_cache.TryGetValue(
                    cacheKey,
                    out HistoricalRateResponse cachedData))
                {
                    _loggingService.LogInfo(
                        "Using cached historical data after exception.");

                    return cachedData;
                }

                return null;
            }
        }

        public virtual async Task<List<AverageRateDto>?> GetAverageRates(
    string startDate,
    string endDate,
    string baseCurrency,
    string symbols)
        {
            try
            {
                var historicalData =
                    await GetHistoricalRates(
                        startDate,
                        endDate,
                        baseCurrency,
                        symbols);

                if (historicalData == null)
                {
                    _loggingService.LogError(
                        "Historical data is null in average calculation.");

                    return null;
                }

                var currencyValues =
                    new Dictionary<string, List<decimal>>();

                foreach (var day in historicalData.Rates)
                {
                    foreach (var currency in day.Value)
                    {
                        if (!currencyValues.ContainsKey(currency.Key))
                        {
                            currencyValues[currency.Key] =
                                new List<decimal>();
                        }

                        currencyValues[currency.Key]
                            .Add(currency.Value);
                    }
                }

                var averages = currencyValues
                    .Select(c => new AverageRateDto
                    {
                        Currency = c.Key,
                        AverageRate = c.Value.Average()
                    })
                    .ToList();

                return averages;
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex.Message);

                return null;
            }
        }
        public virtual async Task<Dictionary<string, string>?> GetCurrencies()
        {
            var cacheKey = "currencies";

            try
            {
                var url =
                    "https://api.frankfurter.dev/v1/currencies";

                var response =
                    await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _loggingService.LogError(
                        $"Currency API error: {response.StatusCode}");

                    if (_cache.TryGetValue(
                        cacheKey,
                        out Dictionary<string, string> cachedCurrencies))
                    {
                        _loggingService.LogInfo(
                            "Using cached currencies.");

                        return cachedCurrencies;
                    }

                    return null;
                }

                var json =
                    await response.Content.ReadAsStringAsync();

                var options =
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                var result =
                    JsonSerializer.Deserialize<
                        Dictionary<string, string>>(
                        json,
                        options);

                if (result != null)
                {
                    _cache.Set(
                        cacheKey,
                        result,
                        TimeSpan.FromHours(24));
                }

                return result;
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex.Message);

                if (_cache.TryGetValue(
                    cacheKey,
                    out Dictionary<string, string> cachedCurrencies))
                {
                    _loggingService.LogInfo(
                        "Using cached currencies after exception.");

                    return cachedCurrencies;
                }

                return null;
            }
        }
    }
}
