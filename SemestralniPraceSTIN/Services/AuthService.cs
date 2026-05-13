using Microsoft.IdentityModel.Tokens;
using SemestralniPraceSTIN.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SemestralniPraceSTIN.Services
{
    // Authentication logic
    public class AuthService
    {
        private readonly AppDbContext _db;

        public AuthService(AppDbContext db)
        {
            _db = db;
        }

        public virtual string? Login(string password)
        {
            var user = _db.User.FirstOrDefault();

            if (user == null || user.PasswordHash != password)
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("super_secret_key_12345_super_secret_key_12345");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim(ClaimTypes.Name, "user")
            }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
