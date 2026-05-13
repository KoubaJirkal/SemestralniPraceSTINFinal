using Microsoft.EntityFrameworkCore;
using SemestralniPraceSTIN.Models;
using SemestralniPraceSTIN.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemestralniPraceSTIN.Tests
{
    public class AuthServiceTests
    {
        [Fact]
        public void Login_ReturnsToken_WhenPasswordIsCorrect()
        {
            var options =
                new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var db =
                new AppDbContext(options);

            db.User.Add(new User
            {
                PasswordHash = "1234",
                BaseCurrency = "EUR",
                SelectedCurrencies = "USD,CZK"
            });

            db.SaveChanges();

            var authService =
                new AuthService(db);

            var result =
                authService.Login("1234");

            Assert.NotNull(result);
        }
        [Fact]
        public void Login_ReturnsNull_WhenPasswordIsIncorrect()
        {
            var options =
                new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var db =
                new AppDbContext(options);

            db.User.Add(new User
            {
                PasswordHash = "1234",
                BaseCurrency = "EUR",
                SelectedCurrencies = "USD,CZK"
            });

            db.SaveChanges();

            var authService =
                new AuthService(db);

            var result =
                authService.Login("wrong");

            Assert.Null(result);
        }
    }
}
