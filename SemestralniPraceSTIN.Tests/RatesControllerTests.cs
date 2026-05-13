using Microsoft.AspNetCore.Mvc;
using Moq;
using SemestralniPraceSTIN.Controllers;
using SemestralniPraceSTIN.DTOs;
using SemestralniPraceSTIN.Services;

namespace SemestralniPraceSTIN.Tests
{
    public class RatesControllerTests
    {
        [Fact]
        public async Task GetStrongest_ReturnsOk()
        {
            var serviceMock =
                new Mock<ExchangeRateService>(
                    null!,
                    null!,
                    null!,
                    null!);

            serviceMock
                .Setup(s => s.GetStrongestCurrency(
                    "EUR",
                    "USD,CZK"))
                .ReturnsAsync(
                    new CurrencyResultDto
                    {
                        Currency = "CZK",
                        Rate = 25
                    });

            var controller =
                new RatesController(
                    serviceMock.Object);

            var result =
                await controller.GetStrongest(
                    "EUR",
                    "USD,CZK");

            var okResult =
                Assert.IsType<OkObjectResult>(
                    result);

            var value =
                Assert.IsType<CurrencyResultDto>(
                    okResult.Value);

            Assert.Equal(
                "CZK",
                value.Currency);
        }

        [Fact]
        public async Task GetStrongest_ReturnsBadRequest_WhenNull()
        {
            var serviceMock =
                new Mock<ExchangeRateService>(
                    null!,
                    null!,
                    null!,
                    null!);

            serviceMock
                .Setup(s => s.GetStrongestCurrency(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync((CurrencyResultDto?)null);

            var controller =
                new RatesController(
                    serviceMock.Object);

            var result =
                await controller.GetStrongest(
                    "EUR",
                    "USD");

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task GetWeakest_ReturnsOk()
        {
            var serviceMock =
                new Mock<ExchangeRateService>(
                    null!,
                    null!,
                    null!,
                    null!);

            serviceMock
                .Setup(s => s.GetWeakestCurrency(
                    "EUR",
                    "USD"))
                .ReturnsAsync(
                    new CurrencyResultDto
                    {
                        Currency = "USD",
                        Rate = 1.1m
                    });

            var controller =
                new RatesController(
                    serviceMock.Object);

            var result =
                await controller.GetWeakest(
                    "EUR",
                    "USD");

            Assert.IsType<OkObjectResult>(
                result);
        }

        [Fact]
        public async Task GetAverage_ReturnsOk()
        {
            var serviceMock =
                new Mock<ExchangeRateService>(
                    null!,
                    null!,
                    null!,
                    null!);

            serviceMock
                .Setup(s => s.GetAverageRates(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(
                    new List<AverageRateDto>
                    {
                        new AverageRateDto
                        {
                            Currency = "USD",
                            AverageRate = 1.5m
                        }
                    });

            var controller =
                new RatesController(
                    serviceMock.Object);

            var result =
                await controller.GetAverage(
                    "2025-01-01",
                    "2025-01-02",
                    "EUR",
                    "USD");

            Assert.IsType<OkObjectResult>(
                result);
        }

        [Fact]
        public async Task GetHistory_ReturnsOk()
        {
            var serviceMock =
                new Mock<ExchangeRateService>(
                    null!,
                    null!,
                    null!,
                    null!);

            serviceMock
                .Setup(s => s.GetHistoricalRates(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(
                    new HistoricalRateResponse());

            var controller =
                new RatesController(
                    serviceMock.Object);

            var result =
                await controller.GetHistory(
                    "2025-01-01",
                    "2025-01-02",
                    "EUR",
                    "USD");

            Assert.IsType<OkObjectResult>(
                result);
        }

        [Fact]
        public async Task GetCurrencies_ReturnsOk()
        {
            var serviceMock =
                new Mock<ExchangeRateService>(
                    null!,
                    null!,
                    null!,
                    null!);

            serviceMock
                .Setup(s => s.GetCurrencies())
                .ReturnsAsync(
                    new Dictionary<string, string>
                    {
                        { "USD", "US Dollar" }
                    });

            var controller =
                new RatesController(
                    serviceMock.Object);

            var result =
                await controller.GetCurrencies();

            Assert.IsType<OkObjectResult>(
                result);
        }
        [Fact]
        public async Task GetWeakest_ReturnsBadRequest_WhenNull()
        {
            var mock =
                new Mock<ExchangeRateService>(
                    null, null, null, null);

            mock.Setup(x =>
                x.GetWeakestCurrency(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync((CurrencyResultDto)null);

            var controller =
                new RatesController(mock.Object);

            var result =
                await controller.GetWeakest();

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task GetCurrencies_ReturnsBadRequest_WhenNull()
        {
            var mock =
                new Mock<ExchangeRateService>(
                    null, null, null, null);

            mock.Setup(x =>
                x.GetCurrencies())
                .ReturnsAsync((Dictionary<string, string>)null);

            var controller =
                new RatesController(mock.Object);

            var result =
                await controller.GetCurrencies();

            Assert.IsType<BadRequestResult>(result);
        }
        [Fact]
        public async Task GetAverage_ReturnsBadRequest_WhenServiceReturnsNull()
        {
            var mock =
                new Mock<ExchangeRateService>(
                    null!, null!, null!, null!);

            mock.Setup(x =>
                x.GetAverageRates(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync((List<AverageRateDto>?)null);

            var controller =
                new RatesController(mock.Object);

            var result =
                await controller.GetAverage(
                    "2025-01-01",
                    "2025-01-02",
                    "EUR",
                    "USD");

            Assert.IsType<BadRequestObjectResult>(result);
        }
        [Fact]
        public async Task GetHistory_ReturnsBadRequest_WhenServiceReturnsNull()
        {
            var mock =
                new Mock<ExchangeRateService>(
                    null!, null!, null!, null!);

            mock.Setup(x =>
                x.GetHistoricalRates(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync((HistoricalRateResponse?)null);

            var controller =
                new RatesController(mock.Object);

            var result =
                await controller.GetHistory(
                    "2025-01-01",
                    "2025-01-02",
                    "EUR",
                    "USD");

            Assert.IsType<BadRequestObjectResult>(result);
        }

    }
}