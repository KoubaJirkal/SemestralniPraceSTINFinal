using Microsoft.AspNetCore.Mvc;
using Moq;
using SemestralniPraceSTIN.Controllers;
using SemestralniPraceSTIN.DTOs;
using SemestralniPraceSTIN.Services;

namespace SemestralniPraceSTIN.Tests
{
    // Coverage improvement tests
    public class AuthControllerTests
    {
        [Fact]
        public void Login_ReturnsOk_WhenSuccess()
        {
            var serviceMock =
                new Mock<AuthService>(null!);

            serviceMock
                .Setup(s => s.Login("1234"))
                .Returns("token");

            var controller =
                new AuthController(
                    serviceMock.Object);

            var dto =
                new LoginDto
                {
                    Password = "1234"
                };

            var result =
                controller.Login(dto);

            Assert.IsType<OkObjectResult>(
                result);
        }

        [Fact]
        public void Login_ReturnsUnauthorized_WhenFail()
        {
            var serviceMock =
                new Mock<AuthService>(null!);

            serviceMock
                .Setup(s => s.Login("wrong"))
                .Returns((string?)null);

            var controller =
                new AuthController(
                    serviceMock.Object);

            var dto =
                new LoginDto
                {
                    Password = "wrong"
                };


            var result =
                controller.Login(dto);

            Assert.IsType<UnauthorizedResult>(
                result);
        }
    }
}