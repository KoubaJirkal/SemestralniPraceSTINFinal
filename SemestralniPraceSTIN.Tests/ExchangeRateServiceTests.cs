using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using SemestralniPraceSTIN.Controllers;
using SemestralniPraceSTIN.DTOs;
using SemestralniPraceSTIN.Models;
using SemestralniPraceSTIN.Services;
using System.Net;
using System.Text;
using Xunit;

namespace SemestralniPraceSTIN.Tests
{
    public class ExchangeRateServiceTests
    {
        [Fact]
        public async Task GetStrongestCurrency_ReturnsStrongest()
        {
            // Arrange

            var responseJson =
                """
                {
                    "base": "EUR",
                    "rates": {
                        "USD": 1.1,
                        "CZK": 25.0,
                        "GBP": 0.8
                    }
                }
                """;

            var handler = new FakeHttpMessageHandler(
                responseJson,
                HttpStatusCode.OK);

            var httpClient =
                new HttpClient(handler);

            var options =
                new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(
                    databaseName: Guid.NewGuid().ToString())
                .Options;

            var db =
                new AppDbContext(options);

            var memoryCache =
                new MemoryCache(
                    new MemoryCacheOptions());

            var loggingService =
                new LoggingService(db);

            var service =
                new ExchangeRateService(
                    httpClient,
                    loggingService,
                    memoryCache,
                    db);

            // Act

            var result =
                await service.GetStrongestCurrency(
                    "EUR",
                    "USD,CZK,GBP");

            // Assert

            Assert.NotNull(result);

            Assert.Equal("CZK", result.Currency);

            Assert.Equal(25.0m, result.Rate);
        }
        [Fact]
        public async Task GetWeakestCurrency_ReturnsWeakest()
        {
            var responseJson =
                """
        {
            "base": "EUR",
            "rates": {
                "USD": 1.1,
                "CZK": 25.0,
                "GBP": 0.8
            }
        }
        """;

            var handler = new FakeHttpMessageHandler(
                responseJson,
                HttpStatusCode.OK);

            var httpClient =
                new HttpClient(handler);

            var options =
                new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(
                    Guid.NewGuid().ToString())
                .Options;

            var db =
                new AppDbContext(options);

            var memoryCache =
                new MemoryCache(
                    new MemoryCacheOptions());

            var loggingService =
                new LoggingService(db);

            var service =
                new ExchangeRateService(
                    httpClient,
                    loggingService,
                    memoryCache,
                    db);

            var result =
                await service.GetWeakestCurrency(
                    "EUR",
                    "USD,CZK,GBP");

            Assert.NotNull(result);

            Assert.Equal("GBP", result.Currency);

            Assert.Equal(0.8m, result.Rate);
        }
        [Fact]
        public async Task GetAverageRates_ReturnsCorrectAverage()
        {
            var responseJson =
                """
        {
            "rates": {
                "2025-01-01": {
                    "USD": 1.0,
                    "CZK": 24.0
                },
                "2025-01-02": {
                    "USD": 2.0,
                    "CZK": 26.0
                }
            }
        }
        """;

            var handler = new FakeHttpMessageHandler(
                responseJson,
                HttpStatusCode.OK);

            var httpClient =
                new HttpClient(handler);

            var options =
                new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var db =
                new AppDbContext(options);

            var memoryCache =
                new MemoryCache(
                    new MemoryCacheOptions());

            var loggingService =
                new LoggingService(db);

            var service =
                new ExchangeRateService(
                    httpClient,
                    loggingService,
                    memoryCache,
                    db);

            var result =
                await service.GetAverageRates(
                    "2025-01-01",
                    "2025-01-02",
                    "EUR",
                    "USD,CZK");

            Assert.NotNull(result);

            var usd =
                result.First(r => r.Currency == "USD");

            var czk =
                result.First(r => r.Currency == "CZK");

            Assert.Equal(1.5m, usd.AverageRate);

            Assert.Equal(25.0m, czk.AverageRate);
        }
        [Fact]
        public async Task GetLatestRates_ReturnsCachedData_WhenApiFails()
        {
            var cache =
                new MemoryCache(
                    new MemoryCacheOptions());

            var cachedResponse =
                new ExchangeRateResponse
                {
                    Base = "EUR",
                    Rates = new Dictionary<string, decimal>
                    {
                { "USD", 1.1m }
                    }
                };

            cache.Set(
                "latest_EUR_USD",
                cachedResponse);

            var handler =
                new FakeHttpMessageHandler(
                    "",
                    HttpStatusCode.InternalServerError);

            var httpClient =
                new HttpClient(handler);

            var options =
                new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var db =
                new AppDbContext(options);

            var loggingService =
                new LoggingService(db);

            var service =
                new ExchangeRateService(
                    httpClient,
                    loggingService,
                    cache,
                    db);

            var result =
                await service.GetLatestRates(
                    "EUR",
                    "USD");

            Assert.NotNull(result);

            Assert.Equal(
                1.1m,
                result.Rates["USD"]);
        }
        [Fact]
        public async Task GetLatestRates_ReturnsNull_WhenApiFailsAndNoCacheExists()
        {
            var handler =
                new FakeHttpMessageHandler(
                    "",
                    HttpStatusCode.InternalServerError);

            var httpClient =
                new HttpClient(handler);

            var options =
                new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var db =
                new AppDbContext(options);

            var memoryCache =
                new MemoryCache(
                    new MemoryCacheOptions());

            var loggingService =
                new LoggingService(db);

            var service =
                new ExchangeRateService(
                    httpClient,
                    loggingService,
                    memoryCache,
                    db);

            var result =
                await service.GetLatestRates(
                    "EUR",
                    "USD");

            Assert.Null(result);
        }
        [Fact]
        public async Task GetStrongestCurrency_ReturnsNull_WhenRatesEmpty()
        {
            var responseJson =
            """
    {
        "base": "EUR",
        "rates": {}
    }
    """;

            var handler = new FakeHttpMessageHandler(
                responseJson,
                HttpStatusCode.OK);

            var httpClient = new HttpClient(handler);

            var options =
                new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var db = new AppDbContext(options);

            var memoryCache =
                new MemoryCache(new MemoryCacheOptions());

            var loggingService =
                new LoggingService(db);

            var service =
                new ExchangeRateService(
                    httpClient,
                    loggingService,
                    memoryCache,
                    db);

            var result =
                await service.GetStrongestCurrency(
                    "EUR",
                    "USD");

            Assert.Null(result);
        }
        [Fact]
        public async Task GetStrongestCurrency_ReturnsNull_WhenApiFails()
        {
            var handler = new FakeHttpMessageHandler(
                "",
                HttpStatusCode.InternalServerError);

            var httpClient = new HttpClient(handler);

            var options =
                new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var db = new AppDbContext(options);

            var memoryCache =
                new MemoryCache(new MemoryCacheOptions());

            var loggingService =
                new LoggingService(db);

            var service =
                new ExchangeRateService(
                    httpClient,
                    loggingService,
                    memoryCache,
                    db);

            var result =
                await service.GetStrongestCurrency(
                    "EUR",
                    "USD");

            Assert.Null(result);
        }
        [Fact]
        public async Task GetHistoricalRates_ReturnsData()
        {
            var responseJson =
            """
    {
      "rates": {
        "2025-01-01": {
          "USD": 1.1
        }
      }
    }
    """;

            var handler =
                new FakeHttpMessageHandler(
                    responseJson,
                    HttpStatusCode.OK);

            var httpClient =
                new HttpClient(handler);

            var options =
                new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var db = new AppDbContext(options);

            var memoryCache =
                new MemoryCache(new MemoryCacheOptions());

            var loggingService =
                new LoggingService(db);

            var service =
                new ExchangeRateService(
                    httpClient,
                    loggingService,
                    memoryCache,
                    db);

            var result =
                await service.GetHistoricalRates(
                    "EUR",
                    "USD",
                    "2025-01-01",
                    "2025-01-02");

            Assert.NotNull(result);
        }
        [Fact]
        public async Task GetCurrencies_ReturnsCurrencies()
        {
            var responseJson =
                """
                {
                  "USD": "US Dollar",
                  "CZK": "Czech Koruna",
                  "GBP": "British Pound"
                }
                """;

            var handler =
                new FakeHttpMessageHandler(
                    responseJson,
                    HttpStatusCode.OK);

            var httpClient =
                new HttpClient(handler);

            var options =
                new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var db = new AppDbContext(options);

            var memoryCache =
                new MemoryCache(new MemoryCacheOptions());

            var loggingService =
                new LoggingService(db);

            var service =
                new ExchangeRateService(
                    httpClient,
                    loggingService,
                    memoryCache,
                    db);

            var result =
                await service.GetCurrencies();

            Assert.NotNull(result);

            Assert.True(
                result.ContainsKey("USD"));
        }
        [Fact]
        public void LoggingService_AddsLog()
        {
            var options =
                new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var db = new AppDbContext(options);

            var service =
                new LoggingService(db);

            service.LogError("test");

            Assert.Single(db.Logs);
        }
        [Fact]
        public async Task GetCurrencies_ReturnsEmpty_WhenJsonInvalid()
        {
            var handler =
                new FakeHttpMessageHandler(
                    "invalid json",
                    HttpStatusCode.OK);

            var httpClient =
                new HttpClient(handler);

            var options =
                new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var db = new AppDbContext(options);

            var cache =
                new MemoryCache(new MemoryCacheOptions());

            var loggingService =
                new LoggingService(db);

            var service =
                new ExchangeRateService(
                    httpClient,
                    loggingService,
                    cache,
                    db);

            var result =
                await service.GetCurrencies();

            Assert.Null(result);
        }
        [Fact]
        public async Task GetCurrencies_ReturnsEmpty_WhenSymbolsEmpty()
        {
            var responseJson =
            """
    {
        "symbols": {}
    }
    """;

            var handler =
                new FakeHttpMessageHandler(
                    responseJson,
                    HttpStatusCode.OK);

            var httpClient =
                new HttpClient(handler);

            var options =
                new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var db = new AppDbContext(options);

            var cache =
                new MemoryCache(new MemoryCacheOptions());

            var loggingService =
                new LoggingService(db);

            var service =
                new ExchangeRateService(
                    httpClient,
                    loggingService,
                    cache,
                    db);

            var result =
                await service.GetCurrencies();

            Assert.Null(result);
        }
        [Fact]
        public async Task GetCurrencies_ReturnsEmpty_WhenSymbolsNull()
        {
            var responseJson =
            """
    {
        "symbols": null
    }
    """;

            var handler =
                new FakeHttpMessageHandler(
                    responseJson,
                    HttpStatusCode.OK);

            var httpClient =
                new HttpClient(handler);

            var options =
                new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var db = new AppDbContext(options);

            var cache =
                new MemoryCache(new MemoryCacheOptions());

            var loggingService =
                new LoggingService(db);

            var service =
                new ExchangeRateService(
                    httpClient,
                    loggingService,
                    cache,
                    db);

            var result =
                await service.GetCurrencies();

            Assert.NotNull(result);
        }
        [Fact]
        public async Task GetHistoricalRates_ReturnsEmpty_WhenRatesEmpty()
        {
            var responseJson =
            """
    {
        "rates": {}
    }
    """;

            var handler =
                new FakeHttpMessageHandler(
                    responseJson,
                    HttpStatusCode.OK);

            var httpClient =
                new HttpClient(handler);

            var options =
                new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var db = new AppDbContext(options);

            var cache =
                new MemoryCache(new MemoryCacheOptions());

            var loggingService =
                new LoggingService(db);

            var service =
                new ExchangeRateService(
                    httpClient,
                    loggingService,
                    cache,
                    db);

            var result =
                await service.GetHistoricalRates(
                    "EUR",
                    "USD",
                    "2025-01-01",
                    "2025-01-02");

            Assert.NotNull(result);

            Assert.Empty(result.Rates);
        }
        [Fact]
        public async Task GetAverageRates_ReturnsEmpty_WhenNoData()
        {
            var responseJson =
            """
    {
        "rates": {}
    }
    """;

            var handler =
                new FakeHttpMessageHandler(
                    responseJson,
                    HttpStatusCode.OK);

            var httpClient =
                new HttpClient(handler);

            var options =
                new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var db = new AppDbContext(options);

            var cache =
                new MemoryCache(new MemoryCacheOptions());

            var loggingService =
                new LoggingService(db);

            var service =
                new ExchangeRateService(
                    httpClient,
                    loggingService,
                    cache,
                    db);

            var result =
                await service.GetAverageRates(
                    "2025-01-01",
                    "2025-01-02",
                    "EUR",
                    "USD");

            Assert.Empty(result);
        }
        [Fact]
        public async Task GetLatestRates_LogsError_WhenApiFails()
        {
            var handler =
                new FakeHttpMessageHandler(
                    "",
                    HttpStatusCode.InternalServerError);

            var httpClient =
                new HttpClient(handler);

            var options =
                new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var db = new AppDbContext(options);

            var cache =
                new MemoryCache(new MemoryCacheOptions());

            var loggingService =
                new LoggingService(db);

            var service =
                new ExchangeRateService(
                    httpClient,
                    loggingService,
                    cache,
                    db);

            await service.GetLatestRates(
                "EUR",
                "USD");

            Assert.NotEmpty(db.Logs);
        }
        [Fact]
        public async Task GetLatestRates_ReturnsNull_WhenJsonInvalid()
        {
            var handler =
                new FakeHttpMessageHandler(
                    "invalid json",
                    HttpStatusCode.OK);

            var httpClient =
                new HttpClient(handler);

            var options =
                new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var db =
                new AppDbContext(options);

            var cache =
                new MemoryCache(new MemoryCacheOptions());

            var logging =
                new LoggingService(db);

            var service =
                new ExchangeRateService(
                    httpClient,
                    logging,
                    cache,
                    db);

            var result =
                await service.GetLatestRates(
                    "EUR",
                    "USD");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetWeakestCurrency_ReturnsNull_WhenRatesEmpty()
        {
            var responseJson =
            """
    {
        "base": "EUR",
        "rates": {}
    }
    """;

            var handler =
                new FakeHttpMessageHandler(
                    responseJson,
                    HttpStatusCode.OK);

            var httpClient =
                new HttpClient(handler);

            var options =
                new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var db =
                new AppDbContext(options);

            var cache =
                new MemoryCache(new MemoryCacheOptions());

            var logging =
                new LoggingService(db);

            var service =
                new ExchangeRateService(
                    httpClient,
                    logging,
                    cache,
                    db);

            var result =
                await service.GetWeakestCurrency(
                    "EUR",
                    "USD");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetWeakestCurrency_ReturnsNull_WhenApiFails()
        {
            var handler =
                new FakeHttpMessageHandler(
                    "",
                    HttpStatusCode.InternalServerError);

            var httpClient =
                new HttpClient(handler);

            var options =
                new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var db =
                new AppDbContext(options);

            var cache =
                new MemoryCache(new MemoryCacheOptions());

            var logging =
                new LoggingService(db);

            var service =
                new ExchangeRateService(
                    httpClient,
                    logging,
                    cache,
                    db);

            var result =
                await service.GetWeakestCurrency(
                    "EUR",
                    "USD");

            Assert.Null(result);
        }
        [Fact]
        public async Task GetAverage_ReturnsBadRequest_WhenServiceReturnsNull()
        {
            var mockService =
                new Mock<ExchangeRateService>(
                    null,
                    null,
                    null,
                    null);

            mockService
                .Setup(x => x.GetAverageRates(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync((List<AverageRateDto>)null);

            var controller =
                new RatesController(mockService.Object);

            var result =
                await controller.GetAverage(
                    "2025-01-01",
                    "2025-01-02");

            Assert.IsType<BadRequestObjectResult>(result);
        }
        [Fact]
        public async Task GetAverageRates_ReturnsEmpty_WhenRatesMissing()
        {
            var responseJson =
            """
    {
        "rates": {}
    }
    """;

            var handler =
                new FakeHttpMessageHandler(
                    responseJson,
                    HttpStatusCode.OK);

            var httpClient =
                new HttpClient(handler);

            var options =
                new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var db =
                new AppDbContext(options);

            var cache =
                new MemoryCache(new MemoryCacheOptions());

            var logging =
                new LoggingService(db);

            var service =
                new ExchangeRateService(
                    httpClient,
                    logging,
                    cache,
                    db);

            var result =
                await service.GetAverageRates(
                    "2025-01-01",
                    "2025-01-02",
                    "EUR",
                    "USD");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetHistoricalRates_ReturnsNull_WhenApiFails()
        {
            var handler =
                new FakeHttpMessageHandler(
                    "",
                    HttpStatusCode.InternalServerError);

            var httpClient =
                new HttpClient(handler);

            var options =
                new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var db =
                new AppDbContext(options);

            var cache =
                new MemoryCache(new MemoryCacheOptions());

            var logging =
                new LoggingService(db);

            var service =
                new ExchangeRateService(
                    httpClient,
                    logging,
                    cache,
                    db);

            var result =
                await service.GetHistoricalRates(
                    "EUR",
                    "USD",
                    "2025-01-01",
                    "2025-01-02");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAverageRates_ReturnsNull_WhenApiFails()
        {
            var handler =
                new FakeHttpMessageHandler(
                    "",
                    HttpStatusCode.InternalServerError);

            var httpClient =
                new HttpClient(handler);

            var options =
                new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var db =
                new AppDbContext(options);

            var cache =
                new MemoryCache(new MemoryCacheOptions());

            var logging =
                new LoggingService(db);

            var service =
                new ExchangeRateService(
                    httpClient,
                    logging,
                    cache,
                    db);

            var result =
                await service.GetAverageRates(
                    "2025-01-01",
                    "2025-01-02",
                    "EUR",
                    "USD");

            Assert.Null(result);
        }
        [Fact]
        public async Task GetCurrencies_ReturnsNull_WhenApiFails()
        {
            var handler =
                new FakeHttpMessageHandler(
                    "",
                    HttpStatusCode.InternalServerError);

            var httpClient =
                new HttpClient(handler);

            var options =
                new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var db =
                new AppDbContext(options);

            var cache =
                new MemoryCache(new MemoryCacheOptions());

            var logging =
                new LoggingService(db);

            var service =
                new ExchangeRateService(
                    httpClient,
                    logging,
                    cache,
                    db);

            var result =
                await service.GetCurrencies();

            Assert.Null(result);
        }

        [Fact]
        public async Task GetHistoricalRates_ReturnsNull_WhenJsonInvalid()
        {
            var handler =
                new FakeHttpMessageHandler(
                    "invalid json",
                    HttpStatusCode.OK);

            var httpClient =
                new HttpClient(handler);

            var options =
                new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var db =
                new AppDbContext(options);

            var cache =
                new MemoryCache(new MemoryCacheOptions());

            var logging =
                new LoggingService(db);

            var service =
                new ExchangeRateService(
                    httpClient,
                    logging,
                    cache,
                    db);

            var result =
                await service.GetHistoricalRates(
                    "EUR",
                    "USD",
                    "2025-01-01",
                    "2025-01-02");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAverageRates_ReturnsNull_WhenJsonInvalid()
        {
            var handler =
                new FakeHttpMessageHandler(
                    "invalid json",
                    HttpStatusCode.OK);

            var httpClient =
                new HttpClient(handler);

            var options =
                new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var db =
                new AppDbContext(options);

            var cache =
                new MemoryCache(new MemoryCacheOptions());

            var logging =
                new LoggingService(db);

            var service =
                new ExchangeRateService(
                    httpClient,
                    logging,
                    cache,
                    db);

            var result =
                await service.GetAverageRates(
                    "2025-01-01",
                    "2025-01-02",
                    "EUR",
                    "USD");

            Assert.Null(result);
        }

    }

}