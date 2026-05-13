using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using smestralka.DTOs;
using smestralka.Models;

namespace smestralka.Controllers
{

    // User settings endpoint
    [ApiController]
    [Route("api/settings")]
    [Authorize]
    public class SettingsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public SettingsController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult GetSettings()
        {
            var user = _db.User.FirstOrDefault();

            if (user == null)
                return NotFound();

            return Ok(new UserSettingsDto
            {
                BaseCurrency = user.BaseCurrency,
                SelectedCurrencies = user.SelectedCurrencies
            });
        }

        [HttpPut]
        public IActionResult UpdateSettings(
            UserSettingsDto dto)
        {
            var user = _db.User.FirstOrDefault();

            if (user == null)
                return NotFound();

            user.BaseCurrency = dto.BaseCurrency;
            user.SelectedCurrencies = dto.SelectedCurrencies;

            _db.SaveChanges();

            return Ok(dto);
        }
    }
}
