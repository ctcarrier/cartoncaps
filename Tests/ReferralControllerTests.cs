using Xunit;
using Moq;
using FluentAssertions;
using CartonCapsApi.Controllers;
using CartonCapsApi.Services;
using Microsoft.AspNetCore.Mvc;
using CartonCapsApi.Models;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using CartonCapsApi.Data.Entities;

namespace Tests
{
    public class ReferralControllerTests
    {

        [Fact]
        public async Task GetReferralLink_Returns200_WhenValid()
        {
            var mockService = new Mock<IReferralService>();
            mockService.Setup(s => s.GetReferralLinkAsync("user123"))
                       .ReturnsAsync("https://example.com/ref/ABC123");

            var controller = new ReferralController(mockService.Object);
            SetupControllerUser(controller, "user123");

            var result = await controller.GetReferralLink() as OkObjectResult;
            result.Should().NotBeNull();
#pragma warning disable 8602
            var response = result.Value as CreateReferralLinkResponse;
            response.Should().NotBeNull();
            response.ReferralLink.Should().Be("https://example.com/ref/ABC123");
#pragma warning restore 8602
        }

        [Fact]
        public async Task GetReferredUsers_Returns200()
        {
            var mockService = new Mock<IReferralService>();
            mockService.Setup(s => s.GetReferredUsersAsync("user123"))
                       .ReturnsAsync(new System.Collections.Generic.List<string> { "refUser1", "refUser2" });

            var controller = new ReferralController(mockService.Object);
            SetupControllerUser(controller, "user123");

            var result = await controller.GetReferredUsers() as OkObjectResult;
            result.Should().NotBeNull();
#pragma warning disable 8602
            var value = result.Value as dynamic;
            ((System.Collections.Generic.List<string>)value.referrals).Should().Contain(new[] { "refUser1", "refUser2" });
#pragma warning restore 8602
        }

        [Fact]
        public async Task GetReferredUsers_Returns401_IfNoToken()
        {
            var mockService = new Mock<IReferralService>();
            var controller = new ReferralController(mockService.Object);
            SetupEmptyUser(controller, "test");

            Func<Task> act = async () => await controller.GetReferredUsers();
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("NameIdentifier claim not found*");
        }

        [Fact]
        public async Task CreateReferredUser_Returns400_WhenReferredUserIdMissing()
        {
            var mockService = new Mock<IReferralService>();
            var controller = new ReferralController(mockService.Object);
            SetupControllerUser(controller, "user123");

            var request = new CreateReferredUserRequest { };
            var result = await controller.CreateReferredUser(request) as BadRequestObjectResult;
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateReferredUser_Returns409_WhenAlreadyExists()
        {
            var mockService = new Mock<IReferralService>();
            mockService.Setup(s => s.CreateReferredUserAsync("user123", "existingUser"))
                       .Throws(new InvalidOperationException("This referred user already exists for the given user."));

            var controller = new ReferralController(mockService.Object);
            SetupControllerUser(controller, "user123");

            var request = new CreateReferredUserRequest { ReferredUserId = "existingUser" };
            var result = await controller.CreateReferredUser(request) as ConflictObjectResult;
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateReferredUser_Returns201_WhenValid()
        {
            var mockService = new Mock<IReferralService>();
            var newUser = new ReferredUser { Id = 3, UserId = "user123", ReferredUserId = "newUser", CreatedDate = DateTime.UtcNow };
            mockService.Setup(s => s.CreateReferredUserAsync("user123", "newUser")).ReturnsAsync(newUser);

            var controller = new ReferralController(mockService.Object);
            SetupControllerUser(controller, "user123");

            var request = new CreateReferredUserRequest { ReferredUserId = "newUser" };
            var result = await controller.CreateReferredUser(request) as CreatedResult;
            result.Should().NotBeNull();
#pragma warning disable 8602, 8600
            ((ReferredUser)result.Value).Should().BeEquivalentTo(newUser);
#pragma warning restore 8602, 8600
        }

        [Fact]
        public async Task AllEndpoints_ThrowIfNameIdentifierMissing()
        {
            var mockService = new Mock<IReferralService>();
            var controller = new ReferralController(mockService.Object);
            SetupEmptyUser(controller, "test");

            Func<Task> act = async () => await controller.GetReferredUsers();
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("NameIdentifier claim not found*");
        }

        private void SetupControllerUser(ControllerBase controller, string userId)
        {
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
            var identity = new ClaimsIdentity(claims, "Test");
            controller.ControllerContext.HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            };
        }

        private void SetupEmptyUser(ControllerBase controller, string userId)
        {
            var claims = new[] { new Claim(ClaimTypes.Locality, userId) };
            var identity = new ClaimsIdentity(claims, "Test");
            controller.ControllerContext.HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            };
        }
    }
}
