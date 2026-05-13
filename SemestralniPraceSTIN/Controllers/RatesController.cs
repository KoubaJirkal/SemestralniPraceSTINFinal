using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SemestralniPraceSTIN.Services;

namespace SemestralniPraceSTIN.Controllers
{

    // Historical exchange rates endpoint
    [ApiController]
    [Route("api/rates")]
    [Authorize]
    public class RatesController : ControllerBase
    {
        private readonly ExchangeRateService _exchangeRateService;

        public RatesController(ExchangeRateService exchangeRateService)
        {
            _exchangeRateService = exchangeRateService;
        }

        [HttpGet("latest")]
        public async Task<IActionResult> GetLatest(
            string baseCurrency = "EUR",
            string symbols = "USD,CZK")
        {
            var result = await _exchangeRateService.GetLatestRates(
                baseCurrency,
                symbols);

            if (result == null)
                return BadRequest("Nepodařilo se získat data z API.");

            return Ok(result);
        }

        [HttpGet("strongest")]
        public async Task<IActionResult> GetStrongest(
        string baseCurrency = "EUR",
        string symbols = "USD,CZK,GBP")
        {
            var result = await _exchangeRateService
                .GetStrongestCurrency(baseCurrency, symbols);

            if (result == null)
                return BadRequest();

            return Ok(result);
        }

        [HttpGet("weakest")]
        public async Task<IActionResult> GetWeakest(
            string baseCurrency = "EUR",
            string symbols = "USD,CZK,GBP")
        {
            var result = await _exchangeRateService
                .GetWeakestCurrency(baseCurrency, symbols);

            if (result == null)
                return BadRequest();

            return Ok(result);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory(
        string startDate,
        string endDate,
        string baseCurrency = "EUR",
        string symbols = "USD,CZK")
        {
            var result = await _exchangeRateService.GetHistoricalRates(
                startDate,
                endDate,
                baseCurrency,
                symbols);

            if (result == null)
                return BadRequest("Nepodařilo se získat historická data.");

            return Ok(result);
        }

        [HttpGet("average")]
        public async Task<IActionResult> GetAverage(
        string startDate,
        string endDate,
        string baseCurrency = "EUR",
        string symbols = "USD,CZK")
        {
            var result = await _exchangeRateService.GetAverageRates(
                startDate,
                endDate,
                baseCurrency,
                symbols);

            if (result == null)
                return BadRequest("Nepodařilo se spočítat průměr.");

            return Ok(result);
        }
        [HttpGet("currencies")]
        public async Task<IActionResult> GetCurrencies()
        {
            var result =
                await _exchangeRateService.GetCurrencies();

            if (result == null)
                return BadRequest();

            return Ok(result);
        }
    }
}