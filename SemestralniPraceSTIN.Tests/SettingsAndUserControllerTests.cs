using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SemestralniPraceSTIN.Controllers;
using SemestralniPraceSTIN.DTOs;
using SemestralniPraceSTIN.Models;

namespace SemestralniPraceSTIN.Tests
{
    public class SettingsAndUserControllerTests
    {
        private AppDbContext CreateDbContext()
        {
            var options =
                new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        // =========================
        // SETTINGS CONTROLLER
        // =========================

        [Fact]
        public void GetSettings_ReturnsNotFound_WhenUserMissing()
        {
            var db = CreateDbContext();

            var controller =
                new SettingsController(db);

            var result =
                controller.GetSettings();

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void GetSettings_ReturnsOk_WhenUserExists()
        {
            var db = CreateDbContext();

            db.User.Add(new User
            {
                BaseCurrency = "EUR",
                SelectedCurrencies = "USD,CZK",
                PasswordHash = "testhash"
            });

            db.SaveChanges();

            var controller =
                new SettingsController(db);

            var result =
                controller.GetSettings();

            var ok =
                Assert.IsType<OkObjectResult>(result);

            var dto =
                Assert.IsType<UserSettingsDto>(ok.Value);

            Assert.Equal("EUR", dto.BaseCurrency);
        }

        [Fact]
        public void UpdateSettings_ReturnsNotFound_WhenUserMissing()
        {
            var db = CreateDbContext();

            var controller =
                new SettingsController(db);

            var dto =
                new UserSettingsDto
                {
                    BaseCurrency = "USD",
                    SelectedCurrencies = "GBP"
                };

            var result =
                controller.UpdateSettings(dto);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void UpdateSettings_UpdatesUser_WhenUserExists()
        {
            var db = CreateDbContext();

            db.User.Add(new User
            {
                BaseCurrency = "EUR",
                SelectedCurrencies = "USD",
                PasswordHash = "testhash"
            });

            db.SaveChanges();

            var controller =
                new SettingsController(db);

            var dto =
                new UserSettingsDto
                {
                    BaseCurrency = "CZK",
                    SelectedCurrencies = "GBP"
                };

            var result =
                controller.UpdateSettings(dto);

            var ok =
                Assert.IsType<OkObjectResult>(result);

            var returnedDto =
                Assert.IsType<UserSettingsDto>(ok.Value);

            Assert.Equal("CZK", returnedDto.BaseCurrency);

            var user =
                db.User.First();

            Assert.Equal("CZK", user.BaseCurrency);
            Assert.Equal("GBP", user.SelectedCurrencies);
        }

        // =========================
        // USER CONTROLLER
        // =========================

        [Fact]
        public void User_GetSettings_ReturnsOk_WhenUserExists()
        {
            var db = CreateDbContext();

            db.User.Add(new User
            {
                BaseCurrency = "EUR",
                SelectedCurrencies = "USD",
                PasswordHash = "testhash"
            });

            db.SaveChanges();

            var controller =
                new UserController(db);

            var result =
                controller.GetSettings();

            var ok =
                Assert.IsType<OkObjectResult>(result);

            Assert.NotNull(ok.Value);
        }

        [Fact]
        public void User_GetSettings_ReturnsOk_WithNull_WhenUserMissing()
        {
            var db = CreateDbContext();

            var controller =
                new UserController(db);

            var result =
                controller.GetSettings();

            var ok =
                Assert.IsType<OkObjectResult>(result);

            Assert.Null(ok.Value);
        }

        [Fact]
        public void UpdateSettings_AddsUser_WhenNoUserExists()
        {
            var db = CreateDbContext();

            var controller =
                new UserController(db);

            var settings =
                new User
                {
                    BaseCurrency = "USD",
                    SelectedCurrencies = "GBP",
                    PasswordHash = "testhash"
                };

            var result =
                controller.UpdateSettings(settings);

            Assert.IsType<OkResult>(result);

            Assert.Single(db.User);
        }

        [Fact]
        public void UpdateSettings_UpdatesExistingUser()
        {
            var db = CreateDbContext();

            db.User.Add(new User
            {
                BaseCurrency = "EUR",
                SelectedCurrencies = "USD",
                PasswordHash = "testhash"
            });

            db.SaveChanges();

            var controller =
                new UserController(db);

            var settings =
                new User
                {
                    BaseCurrency = "CZK",
                    SelectedCurrencies = "GBP",
                    PasswordHash = "testhash"
                };

            var result =
                controller.UpdateSettings(settings);

            Assert.IsType<OkResult>(result);

            var user =
                db.User.First();

            Assert.Equal("CZK", user.BaseCurrency);
            Assert.Equal("GBP", user.SelectedCurrencies);
        }
    }
}