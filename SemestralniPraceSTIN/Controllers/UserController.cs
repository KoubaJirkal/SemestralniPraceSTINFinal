using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SemestralniPraceSTIN.Models;

namespace SemestralniPraceSTIN.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _db;

        public UserController(AppDbContext db)
        {
            _db = db;
        }

    
        [Authorize]
        [HttpGet("settings")]
        public IActionResult GetSettings()
        {
            var user = _db.User.FirstOrDefault();

            return Ok(user);
        }



        [HttpPost("settings")]
        public IActionResult UpdateSettings(User settings)
        {
            var user = _db.User.FirstOrDefault();

            if (user == null)
            {
                _db.User.Add(settings);
            }
            else
            {
                user.BaseCurrency = settings.BaseCurrency;
                user.SelectedCurrencies = settings.SelectedCurrencies;
            }

            _db.SaveChanges();

            return Ok();
        }
    }
}
