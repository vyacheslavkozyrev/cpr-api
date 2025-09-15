using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CPR.Api.Controllers;
using CPR.Application.Contracts;
using CPR.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace CPR.UnitTests
{
    public class TaxonomyControllerTests
    {
        private TaxonomyController CreateControllerWithMockedService(out Mock<IClassificationService> svcMock)
        {
            svcMock = new Mock<IClassificationService>();
            svcMock.Setup(s => s.GetCareerPathsAsync()).ReturnsAsync(new CareerPathDto[] { new CareerPathDto { Id = Guid.NewGuid(), Title = "P" } });
            svcMock.Setup(s => s.GetCareerTracksAsync(It.IsAny<Guid?>())).ReturnsAsync(new CareerTrackDto[] { new CareerTrackDto { Id = Guid.NewGuid(), Title = "T", CareerPathId = Guid.NewGuid() } });
            svcMock.Setup(s => s.GetPositionsAsync(It.IsAny<Guid?>())).ReturnsAsync(new PositionDto[] { new PositionDto { Id = Guid.NewGuid(), Title = "Pos", CareerTrackId = Guid.NewGuid(), Expectations = "e" } });

            var controller = new TaxonomyController(svcMock.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
            };
            return controller;
        }

        [Fact]
        public async Task GetCareer_ReturnsOk_WithItems()
        {
            var ctrl = CreateControllerWithMockedService(out var mock);
            var res = await ctrl.GetCareer();
            var ok = Assert.IsType<OkObjectResult>(res);
            var arr = Assert.IsType<CareerPathDto[]>(ok.Value);
            Assert.Single(arr);
        }

        [Fact]
        public async Task GetCareerTracks_ReturnsOk_WithItems()
        {
            var ctrl = CreateControllerWithMockedService(out var mock);
            var res = await ctrl.GetCareerTracks(null);
            var ok = Assert.IsType<OkObjectResult>(res);
            var arr = Assert.IsType<CareerTrackDto[]>(ok.Value);
            Assert.Single(arr);
        }

        [Fact]
        public async Task GetPositions_ReturnsOk_WithExpectations()
        {
            var ctrl = CreateControllerWithMockedService(out var mock);
            var res = await ctrl.GetPositions(null);
            var ok = Assert.IsType<OkObjectResult>(res);
            var arr = Assert.IsType<PositionDto[]>(ok.Value);
            Assert.Single(arr);
            Assert.Equal("e", arr[0].Expectations);
        }
    }
}
