#nullable enable
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CPR.Api.Controllers;
using CPR.Api.Models;
using CPR.Api.Services;
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
        private readonly Guid _testUserId = Guid.NewGuid();

        private TaxonomyController CreateControllerWithMockedService(out Mock<IClassificationService> svcMock)
        {
            var userServiceMock = new Mock<IUserService>();
            svcMock = new Mock<IClassificationService>();
            svcMock.Setup(s => s.GetCareerPathsAsync()).ReturnsAsync(new CareerPathDto[] { new CareerPathDto { Id = Guid.NewGuid(), Title = "P" } });
            svcMock.Setup(s => s.GetCareerTracksAsync(It.IsAny<Guid?>())).ReturnsAsync(new CareerTrackDto[] { new CareerTrackDto { Id = Guid.NewGuid(), Title = "T", CareerPathId = Guid.NewGuid() } });
            svcMock.Setup(s => s.GetPositionsAsync(It.IsAny<Guid?>())).ReturnsAsync(new PositionDto[] { new PositionDto { Id = Guid.NewGuid(), Title = "Pos", CareerTrackId = Guid.NewGuid(), Expectations = "e" } });

            var controller = new TaxonomyController(userServiceMock.Object, svcMock.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
            };
            return controller;
        }

        private TaxonomyController CreateAuthenticatedController(
            out Mock<IUserService> userServiceMock,
            out Mock<IClassificationService> svcMock)
        {
            userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(s => s.GetCurrentUserProfileAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new UserProfile { UserId = _testUserId.ToString(), EmployeeId = Guid.NewGuid().ToString() });

            svcMock = new Mock<IClassificationService>();

            var controller = new TaxonomyController(userServiceMock.Object, svcMock.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
            };
            return controller;
        }

        private TaxonomyController CreateUnauthenticatedController(
            out Mock<IUserService> userServiceMock,
            out Mock<IClassificationService> svcMock)
        {
            userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(s => s.GetCurrentUserProfileAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((UserProfile?)null);

            svcMock = new Mock<IClassificationService>();

            var controller = new TaxonomyController(userServiceMock.Object, svcMock.Object);
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

        [Fact]
        public async Task GetSkillCategories_ReturnsOk_WithItems()
        {
            var ctrl = CreateControllerWithMockedService(out var mock);
            mock.Setup(s => s.GetSkillCategoriesAsync())
                .ReturnsAsync(new[] { new SkillCategoryDto { Id = Guid.NewGuid(), Title = "Technical", Description = "Tech skills" } });

            var res = await ctrl.GetSkillCategories();
            var ok = Assert.IsType<OkObjectResult>(res);
            var arr = Assert.IsAssignableFrom<System.Collections.Generic.IEnumerable<SkillCategoryDto>>(ok.Value);
            Assert.Single(arr);
            Assert.Equal("Technical", arr.First().Title);
        }

        #region Career Path CUD Tests

        [Fact]
        public async Task CreateCareerPath_ReturnsCreated_WhenValid()
        {
            var ctrl = CreateAuthenticatedController(out var userMock, out var svcMock);
            var dto = new CreateCareerPathDto { Title = "New Path", Description = "Description" };
            var expectedResult = new CareerPathDto { Id = Guid.NewGuid(), Title = "New Path", Description = "Description" };

            svcMock.Setup(s => s.CreateCareerPathAsync(dto, _testUserId))
                .ReturnsAsync(expectedResult);

            var res = await ctrl.CreateCareerPath(dto);
            var created = Assert.IsType<CreatedAtActionResult>(res);
            var result = Assert.IsType<CareerPathDto>(created.Value);
            Assert.Equal("New Path", result.Title);
        }

        [Fact]
        public async Task CreateCareerPath_ReturnsUnauthorized_WhenNotAuthenticated()
        {
            var ctrl = CreateUnauthenticatedController(out var userMock, out var svcMock);
            var dto = new CreateCareerPathDto { Title = "New Path", Description = "Description" };

            var res = await ctrl.CreateCareerPath(dto);
            Assert.IsType<UnauthorizedResult>(res);
        }

        [Fact]
        public async Task CreateCareerPath_ReturnsBadRequest_WhenDuplicateTitle()
        {
            var ctrl = CreateAuthenticatedController(out var userMock, out var svcMock);
            var dto = new CreateCareerPathDto { Title = "Duplicate", Description = "Description" };

            svcMock.Setup(s => s.CreateCareerPathAsync(dto, _testUserId))
                .ThrowsAsync(new InvalidOperationException("Career path with this title already exists"));

            var res = await ctrl.CreateCareerPath(dto);
            var bad = Assert.IsType<BadRequestObjectResult>(res);
            Assert.NotNull(bad.Value);
        }

        [Fact]
        public async Task UpdateCareerPath_ReturnsOk_WhenValid()
        {
            var ctrl = CreateAuthenticatedController(out var userMock, out var svcMock);
            var id = Guid.NewGuid();
            var dto = new UpdateCareerPathDto { Title = "Updated Path" };
            var expectedResult = new CareerPathDto { Id = id, Title = "Updated Path", Description = "Original Description" };

            svcMock.Setup(s => s.UpdateCareerPathAsync(id, dto, _testUserId))
                .ReturnsAsync(expectedResult);

            var res = await ctrl.UpdateCareerPath(id, dto);
            var ok = Assert.IsType<OkObjectResult>(res);
            var result = Assert.IsType<CareerPathDto>(ok.Value);
            Assert.Equal("Updated Path", result.Title);
        }

        [Fact]
        public async Task UpdateCareerPath_ReturnsUnauthorized_WhenNotAuthenticated()
        {
            var ctrl = CreateUnauthenticatedController(out var userMock, out var svcMock);
            var dto = new UpdateCareerPathDto { Title = "Updated" };

            var res = await ctrl.UpdateCareerPath(Guid.NewGuid(), dto);
            Assert.IsType<UnauthorizedResult>(res);
        }

        [Fact]
        public async Task UpdateCareerPath_ReturnsBadRequest_WhenNotFound()
        {
            var ctrl = CreateAuthenticatedController(out var userMock, out var svcMock);
            var id = Guid.NewGuid();
            var dto = new UpdateCareerPathDto { Title = "Updated" };

            svcMock.Setup(s => s.UpdateCareerPathAsync(id, dto, _testUserId))
                .ThrowsAsync(new InvalidOperationException("Career path not found"));

            var res = await ctrl.UpdateCareerPath(id, dto);
            var bad = Assert.IsType<BadRequestObjectResult>(res);
            Assert.NotNull(bad.Value);
        }

        [Fact]
        public async Task DeleteCareerPath_ReturnsNoContent_WhenValid()
        {
            var ctrl = CreateAuthenticatedController(out var userMock, out var svcMock);
            var id = Guid.NewGuid();

            svcMock.Setup(s => s.DeleteCareerPathAsync(id, _testUserId))
                .Returns(Task.CompletedTask);

            var res = await ctrl.DeleteCareerPath(id);
            Assert.IsType<NoContentResult>(res);
        }

        [Fact]
        public async Task DeleteCareerPath_ReturnsUnauthorized_WhenNotAuthenticated()
        {
            var ctrl = CreateUnauthenticatedController(out var userMock, out var svcMock);

            var res = await ctrl.DeleteCareerPath(Guid.NewGuid());
            Assert.IsType<UnauthorizedResult>(res);
        }

        [Fact]
        public async Task DeleteCareerPath_ReturnsBadRequest_WhenHasDependencies()
        {
            var ctrl = CreateAuthenticatedController(out var userMock, out var svcMock);
            var id = Guid.NewGuid();

            svcMock.Setup(s => s.DeleteCareerPathAsync(id, _testUserId))
                .ThrowsAsync(new InvalidOperationException("Cannot delete career path with existing career tracks"));

            var res = await ctrl.DeleteCareerPath(id);
            var bad = Assert.IsType<BadRequestObjectResult>(res);
            Assert.NotNull(bad.Value);
        }

        #endregion

        #region Career Track CUD Tests

        [Fact]
        public async Task CreateCareerTrack_ReturnsCreated_WhenValid()
        {
            var ctrl = CreateAuthenticatedController(out var userMock, out var svcMock);
            var careerPathId = Guid.NewGuid();
            var dto = new CreateCareerTrackDto { CareerPathId = careerPathId, Title = "New Track", Description = "Description" };
            var expectedResult = new CareerTrackDto { Id = Guid.NewGuid(), CareerPathId = careerPathId, Title = "New Track", Description = "Description" };

            svcMock.Setup(s => s.CreateCareerTrackAsync(dto, _testUserId))
                .ReturnsAsync(expectedResult);

            var res = await ctrl.CreateCareerTrack(dto);
            var created = Assert.IsType<CreatedAtActionResult>(res);
            var result = Assert.IsType<CareerTrackDto>(created.Value);
            Assert.Equal("New Track", result.Title);
        }

        [Fact]
        public async Task CreateCareerTrack_ReturnsUnauthorized_WhenNotAuthenticated()
        {
            var ctrl = CreateUnauthenticatedController(out var userMock, out var svcMock);
            var dto = new CreateCareerTrackDto { CareerPathId = Guid.NewGuid(), Title = "New Track", Description = "Description" };

            var res = await ctrl.CreateCareerTrack(dto);
            Assert.IsType<UnauthorizedResult>(res);
        }

        [Fact]
        public async Task CreateCareerTrack_ReturnsBadRequest_WhenInvalidCareerPath()
        {
            var ctrl = CreateAuthenticatedController(out var userMock, out var svcMock);
            var dto = new CreateCareerTrackDto { CareerPathId = Guid.NewGuid(), Title = "New Track", Description = "Description" };

            svcMock.Setup(s => s.CreateCareerTrackAsync(dto, _testUserId))
                .ThrowsAsync(new InvalidOperationException("Career path not found"));

            var res = await ctrl.CreateCareerTrack(dto);
            var bad = Assert.IsType<BadRequestObjectResult>(res);
            Assert.NotNull(bad.Value);
        }

        [Fact]
        public async Task UpdateCareerTrack_ReturnsOk_WhenValid()
        {
            var ctrl = CreateAuthenticatedController(out var userMock, out var svcMock);
            var id = Guid.NewGuid();
            var dto = new UpdateCareerTrackDto { Title = "Updated Track" };
            var expectedResult = new CareerTrackDto { Id = id, CareerPathId = Guid.NewGuid(), Title = "Updated Track", Description = "Original" };

            svcMock.Setup(s => s.UpdateCareerTrackAsync(id, dto, _testUserId))
                .ReturnsAsync(expectedResult);

            var res = await ctrl.UpdateCareerTrack(id, dto);
            var ok = Assert.IsType<OkObjectResult>(res);
            var result = Assert.IsType<CareerTrackDto>(ok.Value);
            Assert.Equal("Updated Track", result.Title);
        }

        [Fact]
        public async Task UpdateCareerTrack_ReturnsUnauthorized_WhenNotAuthenticated()
        {
            var ctrl = CreateUnauthenticatedController(out var userMock, out var svcMock);
            var dto = new UpdateCareerTrackDto { Title = "Updated" };

            var res = await ctrl.UpdateCareerTrack(Guid.NewGuid(), dto);
            Assert.IsType<UnauthorizedResult>(res);
        }

        [Fact]
        public async Task UpdateCareerTrack_ReturnsBadRequest_WhenNotFound()
        {
            var ctrl = CreateAuthenticatedController(out var userMock, out var svcMock);
            var id = Guid.NewGuid();
            var dto = new UpdateCareerTrackDto { Title = "Updated" };

            svcMock.Setup(s => s.UpdateCareerTrackAsync(id, dto, _testUserId))
                .ThrowsAsync(new InvalidOperationException("Career track not found"));

            var res = await ctrl.UpdateCareerTrack(id, dto);
            var bad = Assert.IsType<BadRequestObjectResult>(res);
            Assert.NotNull(bad.Value);
        }

        [Fact]
        public async Task DeleteCareerTrack_ReturnsNoContent_WhenValid()
        {
            var ctrl = CreateAuthenticatedController(out var userMock, out var svcMock);
            var id = Guid.NewGuid();

            svcMock.Setup(s => s.DeleteCareerTrackAsync(id, _testUserId))
                .Returns(Task.CompletedTask);

            var res = await ctrl.DeleteCareerTrack(id);
            Assert.IsType<NoContentResult>(res);
        }

        [Fact]
        public async Task DeleteCareerTrack_ReturnsUnauthorized_WhenNotAuthenticated()
        {
            var ctrl = CreateUnauthenticatedController(out var userMock, out var svcMock);

            var res = await ctrl.DeleteCareerTrack(Guid.NewGuid());
            Assert.IsType<UnauthorizedResult>(res);
        }

        [Fact]
        public async Task DeleteCareerTrack_ReturnsBadRequest_WhenHasDependencies()
        {
            var ctrl = CreateAuthenticatedController(out var userMock, out var svcMock);
            var id = Guid.NewGuid();

            svcMock.Setup(s => s.DeleteCareerTrackAsync(id, _testUserId))
                .ThrowsAsync(new InvalidOperationException("Cannot delete career track with existing positions"));

            var res = await ctrl.DeleteCareerTrack(id);
            var bad = Assert.IsType<BadRequestObjectResult>(res);
            Assert.NotNull(bad.Value);
        }

        #endregion

        #region Position CUD Tests

        [Fact]
        public async Task CreatePosition_ReturnsCreated_WhenValid()
        {
            var ctrl = CreateAuthenticatedController(out var userMock, out var svcMock);
            var careerTrackId = Guid.NewGuid();
            var dto = new CreatePositionDto { CareerTrackId = careerTrackId, Title = "Senior Engineer", Description = "Description" };
            var expectedResult = new PositionDto { Id = Guid.NewGuid(), CareerTrackId = careerTrackId, Title = "Senior Engineer", Description = "Description" };

            svcMock.Setup(s => s.CreatePositionAsync(dto, _testUserId))
                .ReturnsAsync(expectedResult);

            var res = await ctrl.CreatePosition(dto);
            var created = Assert.IsType<CreatedAtActionResult>(res);
            var result = Assert.IsType<PositionDto>(created.Value);
            Assert.Equal("Senior Engineer", result.Title);
        }

        [Fact]
        public async Task CreatePosition_ReturnsUnauthorized_WhenNotAuthenticated()
        {
            var ctrl = CreateUnauthenticatedController(out var userMock, out var svcMock);
            var dto = new CreatePositionDto { CareerTrackId = Guid.NewGuid(), Title = "Senior Engineer", Description = "Description" };

            var res = await ctrl.CreatePosition(dto);
            Assert.IsType<UnauthorizedResult>(res);
        }

        [Fact]
        public async Task CreatePosition_ReturnsBadRequest_WhenInvalidCareerTrack()
        {
            var ctrl = CreateAuthenticatedController(out var userMock, out var svcMock);
            var dto = new CreatePositionDto { CareerTrackId = Guid.NewGuid(), Title = "Senior Engineer", Description = "Description" };

            svcMock.Setup(s => s.CreatePositionAsync(dto, _testUserId))
                .ThrowsAsync(new InvalidOperationException("Career track not found"));

            var res = await ctrl.CreatePosition(dto);
            var bad = Assert.IsType<BadRequestObjectResult>(res);
            Assert.NotNull(bad.Value);
        }

        [Fact]
        public async Task UpdatePosition_ReturnsOk_WhenValid()
        {
            var ctrl = CreateAuthenticatedController(out var userMock, out var svcMock);
            var id = Guid.NewGuid();
            var dto = new UpdatePositionDto { Title = "Updated Position" };
            var expectedResult = new PositionDto { Id = id, CareerTrackId = Guid.NewGuid(), Title = "Updated Position", Description = "Original" };

            svcMock.Setup(s => s.UpdatePositionAsync(id, dto, _testUserId))
                .ReturnsAsync(expectedResult);

            var res = await ctrl.UpdatePosition(id, dto);
            var ok = Assert.IsType<OkObjectResult>(res);
            var result = Assert.IsType<PositionDto>(ok.Value);
            Assert.Equal("Updated Position", result.Title);
        }

        [Fact]
        public async Task UpdatePosition_ReturnsUnauthorized_WhenNotAuthenticated()
        {
            var ctrl = CreateUnauthenticatedController(out var userMock, out var svcMock);
            var dto = new UpdatePositionDto { Title = "Updated" };

            var res = await ctrl.UpdatePosition(Guid.NewGuid(), dto);
            Assert.IsType<UnauthorizedResult>(res);
        }

        [Fact]
        public async Task UpdatePosition_ReturnsBadRequest_WhenNotFound()
        {
            var ctrl = CreateAuthenticatedController(out var userMock, out var svcMock);
            var id = Guid.NewGuid();
            var dto = new UpdatePositionDto { Title = "Updated" };

            svcMock.Setup(s => s.UpdatePositionAsync(id, dto, _testUserId))
                .ThrowsAsync(new InvalidOperationException("Position not found"));

            var res = await ctrl.UpdatePosition(id, dto);
            var bad = Assert.IsType<BadRequestObjectResult>(res);
            Assert.NotNull(bad.Value);
        }

        [Fact]
        public async Task DeletePosition_ReturnsNoContent_WhenValid()
        {
            var ctrl = CreateAuthenticatedController(out var userMock, out var svcMock);
            var id = Guid.NewGuid();

            svcMock.Setup(s => s.DeletePositionAsync(id, _testUserId))
                .Returns(Task.CompletedTask);

            var res = await ctrl.DeletePosition(id);
            Assert.IsType<NoContentResult>(res);
        }

        [Fact]
        public async Task DeletePosition_ReturnsUnauthorized_WhenNotAuthenticated()
        {
            var ctrl = CreateUnauthenticatedController(out var userMock, out var svcMock);

            var res = await ctrl.DeletePosition(Guid.NewGuid());
            Assert.IsType<UnauthorizedResult>(res);
        }

        [Fact]
        public async Task DeletePosition_ReturnsBadRequest_WhenHasEmployees()
        {
            var ctrl = CreateAuthenticatedController(out var userMock, out var svcMock);
            var id = Guid.NewGuid();

            svcMock.Setup(s => s.DeletePositionAsync(id, _testUserId))
                .ThrowsAsync(new InvalidOperationException("Cannot delete position with existing employees"));

            var res = await ctrl.DeletePosition(id);
            var bad = Assert.IsType<BadRequestObjectResult>(res);
            Assert.NotNull(bad.Value);
        }

        #endregion

        #region Skill Category CUD Tests

        [Fact]
        public async Task CreateSkillCategory_ReturnsCreated_WhenValid()
        {
            var ctrl = CreateAuthenticatedController(out var userMock, out var svcMock);
            var dto = new CreateSkillCategoryDto { Title = "Technical", Description = "Tech skills" };
            var expectedResult = new SkillCategoryDto { Id = Guid.NewGuid(), Title = "Technical", Description = "Tech skills" };

            svcMock.Setup(s => s.CreateSkillCategoryAsync(dto, _testUserId))
                .ReturnsAsync(expectedResult);

            var res = await ctrl.CreateSkillCategory(dto);
            var created = Assert.IsType<CreatedAtActionResult>(res);
            var result = Assert.IsType<SkillCategoryDto>(created.Value);
            Assert.Equal("Technical", result.Title);
        }

        [Fact]
        public async Task CreateSkillCategory_ReturnsUnauthorized_WhenNotAuthenticated()
        {
            var ctrl = CreateUnauthenticatedController(out var userMock, out var svcMock);
            var dto = new CreateSkillCategoryDto { Title = "Technical", Description = "Tech skills" };

            var res = await ctrl.CreateSkillCategory(dto);
            Assert.IsType<UnauthorizedResult>(res);
        }

        [Fact]
        public async Task CreateSkillCategory_ReturnsBadRequest_WhenDuplicateTitle()
        {
            var ctrl = CreateAuthenticatedController(out var userMock, out var svcMock);
            var dto = new CreateSkillCategoryDto { Title = "Duplicate", Description = "Description" };

            svcMock.Setup(s => s.CreateSkillCategoryAsync(dto, _testUserId))
                .ThrowsAsync(new InvalidOperationException("Skill category with this title already exists"));

            var res = await ctrl.CreateSkillCategory(dto);
            var bad = Assert.IsType<BadRequestObjectResult>(res);
            Assert.NotNull(bad.Value);
        }

        [Fact]
        public async Task UpdateSkillCategory_ReturnsOk_WhenValid()
        {
            var ctrl = CreateAuthenticatedController(out var userMock, out var svcMock);
            var id = Guid.NewGuid();
            var dto = new UpdateSkillCategoryDto { Title = "Updated Category" };
            var expectedResult = new SkillCategoryDto { Id = id, Title = "Updated Category", Description = "Original" };

            svcMock.Setup(s => s.UpdateSkillCategoryAsync(id, dto, _testUserId))
                .ReturnsAsync(expectedResult);

            var res = await ctrl.UpdateSkillCategory(id, dto);
            var ok = Assert.IsType<OkObjectResult>(res);
            var result = Assert.IsType<SkillCategoryDto>(ok.Value);
            Assert.Equal("Updated Category", result.Title);
        }

        [Fact]
        public async Task UpdateSkillCategory_ReturnsUnauthorized_WhenNotAuthenticated()
        {
            var ctrl = CreateUnauthenticatedController(out var userMock, out var svcMock);
            var dto = new UpdateSkillCategoryDto { Title = "Updated" };

            var res = await ctrl.UpdateSkillCategory(Guid.NewGuid(), dto);
            Assert.IsType<UnauthorizedResult>(res);
        }

        [Fact]
        public async Task UpdateSkillCategory_ReturnsBadRequest_WhenNotFound()
        {
            var ctrl = CreateAuthenticatedController(out var userMock, out var svcMock);
            var id = Guid.NewGuid();
            var dto = new UpdateSkillCategoryDto { Title = "Updated" };

            svcMock.Setup(s => s.UpdateSkillCategoryAsync(id, dto, _testUserId))
                .ThrowsAsync(new InvalidOperationException("Skill category not found"));

            var res = await ctrl.UpdateSkillCategory(id, dto);
            var bad = Assert.IsType<BadRequestObjectResult>(res);
            Assert.NotNull(bad.Value);
        }

        [Fact]
        public async Task DeleteSkillCategory_ReturnsNoContent_WhenValid()
        {
            var ctrl = CreateAuthenticatedController(out var userMock, out var svcMock);
            var id = Guid.NewGuid();

            svcMock.Setup(s => s.DeleteSkillCategoryAsync(id, _testUserId))
                .Returns(Task.CompletedTask);

            var res = await ctrl.DeleteSkillCategory(id);
            Assert.IsType<NoContentResult>(res);
        }

        [Fact]
        public async Task DeleteSkillCategory_ReturnsUnauthorized_WhenNotAuthenticated()
        {
            var ctrl = CreateUnauthenticatedController(out var userMock, out var svcMock);

            var res = await ctrl.DeleteSkillCategory(Guid.NewGuid());
            Assert.IsType<UnauthorizedResult>(res);
        }

        [Fact]
        public async Task DeleteSkillCategory_ReturnsBadRequest_WhenHasDependencies()
        {
            var ctrl = CreateAuthenticatedController(out var userMock, out var svcMock);
            var id = Guid.NewGuid();

            svcMock.Setup(s => s.DeleteSkillCategoryAsync(id, _testUserId))
                .ThrowsAsync(new InvalidOperationException("Cannot delete skill category with existing skills"));

            var res = await ctrl.DeleteSkillCategory(id);
            var bad = Assert.IsType<BadRequestObjectResult>(res);
            Assert.NotNull(bad.Value);
        }

        #endregion

        #region Skill CUD Tests

        [Fact]
        public async Task CreateSkill_ReturnsCreated_WhenValid()
        {
            var ctrl = CreateAuthenticatedController(out var userMock, out var svcMock);
            var categoryId = Guid.NewGuid();
            var dto = new CreateSkillDto { CategoryId = categoryId, Title = "C#", Description = "Programming" };
            var expectedResult = new SkillDto { Id = Guid.NewGuid(), CategoryId = categoryId, Title = "C#", Description = "Programming" };

            svcMock.Setup(s => s.CreateSkillAsync(dto, _testUserId))
                .ReturnsAsync(expectedResult);

            var res = await ctrl.CreateSkill(dto);
            var created = Assert.IsType<CreatedAtActionResult>(res);
            var result = Assert.IsType<SkillDto>(created.Value);
            Assert.Equal("C#", result.Title);
        }

        [Fact]
        public async Task CreateSkill_ReturnsUnauthorized_WhenNotAuthenticated()
        {
            var ctrl = CreateUnauthenticatedController(out var userMock, out var svcMock);
            var dto = new CreateSkillDto { CategoryId = Guid.NewGuid(), Title = "C#", Description = "Programming" };

            var res = await ctrl.CreateSkill(dto);
            Assert.IsType<UnauthorizedResult>(res);
        }

        [Fact]
        public async Task CreateSkill_ReturnsBadRequest_WhenInvalidCategory()
        {
            var ctrl = CreateAuthenticatedController(out var userMock, out var svcMock);
            var dto = new CreateSkillDto { CategoryId = Guid.NewGuid(), Title = "C#", Description = "Programming" };

            svcMock.Setup(s => s.CreateSkillAsync(dto, _testUserId))
                .ThrowsAsync(new InvalidOperationException("Skill category not found"));

            var res = await ctrl.CreateSkill(dto);
            var bad = Assert.IsType<BadRequestObjectResult>(res);
            Assert.NotNull(bad.Value);
        }

        [Fact]
        public async Task UpdateSkill_ReturnsOk_WhenValid()
        {
            var ctrl = CreateAuthenticatedController(out var userMock, out var svcMock);
            var id = Guid.NewGuid();
            var dto = new UpdateSkillDto { Title = "Updated Skill" };
            var expectedResult = new SkillDto { Id = id, CategoryId = Guid.NewGuid(), Title = "Updated Skill", Description = "Original" };

            svcMock.Setup(s => s.UpdateSkillAsync(id, dto, _testUserId))
                .ReturnsAsync(expectedResult);

            var res = await ctrl.UpdateSkill(id, dto);
            var ok = Assert.IsType<OkObjectResult>(res);
            var result = Assert.IsType<SkillDto>(ok.Value);
            Assert.Equal("Updated Skill", result.Title);
        }

        [Fact]
        public async Task UpdateSkill_ReturnsUnauthorized_WhenNotAuthenticated()
        {
            var ctrl = CreateUnauthenticatedController(out var userMock, out var svcMock);
            var dto = new UpdateSkillDto { Title = "Updated" };

            var res = await ctrl.UpdateSkill(Guid.NewGuid(), dto);
            Assert.IsType<UnauthorizedResult>(res);
        }

        [Fact]
        public async Task UpdateSkill_ReturnsBadRequest_WhenNotFound()
        {
            var ctrl = CreateAuthenticatedController(out var userMock, out var svcMock);
            var id = Guid.NewGuid();
            var dto = new UpdateSkillDto { Title = "Updated" };

            svcMock.Setup(s => s.UpdateSkillAsync(id, dto, _testUserId))
                .ThrowsAsync(new InvalidOperationException("Skill not found"));

            var res = await ctrl.UpdateSkill(id, dto);
            var bad = Assert.IsType<BadRequestObjectResult>(res);
            Assert.NotNull(bad.Value);
        }

        [Fact]
        public async Task DeleteSkill_ReturnsNoContent_WhenValid()
        {
            var ctrl = CreateAuthenticatedController(out var userMock, out var svcMock);
            var id = Guid.NewGuid();

            svcMock.Setup(s => s.DeleteSkillAsync(id, _testUserId))
                .Returns(Task.CompletedTask);

            var res = await ctrl.DeleteSkill(id);
            Assert.IsType<NoContentResult>(res);
        }

        [Fact]
        public async Task DeleteSkill_ReturnsUnauthorized_WhenNotAuthenticated()
        {
            var ctrl = CreateUnauthenticatedController(out var userMock, out var svcMock);

            var res = await ctrl.DeleteSkill(Guid.NewGuid());
            Assert.IsType<UnauthorizedResult>(res);
        }

        [Fact]
        public async Task DeleteSkill_ReturnsBadRequest_WhenHasDependencies()
        {
            var ctrl = CreateAuthenticatedController(out var userMock, out var svcMock);
            var id = Guid.NewGuid();

            svcMock.Setup(s => s.DeleteSkillAsync(id, _testUserId))
                .ThrowsAsync(new InvalidOperationException("Cannot delete skill with existing skill levels"));

            var res = await ctrl.DeleteSkill(id);
            var bad = Assert.IsType<BadRequestObjectResult>(res);
            Assert.NotNull(bad.Value);
        }

        #endregion

        #region Skill Level CUD Tests

        [Fact]
        public async Task CreateSkillLevel_ReturnsCreated_WhenValid()
        {
            var ctrl = CreateAuthenticatedController(out var userMock, out var svcMock);
            var skillId = Guid.NewGuid();
            var dto = new CreateSkillLevelDto { SkillId = skillId, Value = 3, Title = "Intermediate", Description = "Mid level" };
            var expectedResult = new SkillLevelDto { Id = Guid.NewGuid(), SkillId = skillId, Value = 3, Title = "Intermediate", Description = "Mid level" };

            svcMock.Setup(s => s.CreateSkillLevelAsync(dto, _testUserId))
                .ReturnsAsync(expectedResult);

            var res = await ctrl.CreateSkillLevel(dto);
            var created = Assert.IsType<CreatedAtActionResult>(res);
            var result = Assert.IsType<SkillLevelDto>(created.Value);
            Assert.Equal("Intermediate", result.Title);
            Assert.Equal(3, result.Value);
        }

        [Fact]
        public async Task CreateSkillLevel_ReturnsUnauthorized_WhenNotAuthenticated()
        {
            var ctrl = CreateUnauthenticatedController(out var userMock, out var svcMock);
            var dto = new CreateSkillLevelDto { SkillId = Guid.NewGuid(), Value = 3, Title = "Intermediate", Description = "Mid level" };

            var res = await ctrl.CreateSkillLevel(dto);
            Assert.IsType<UnauthorizedResult>(res);
        }

        [Fact]
        public async Task CreateSkillLevel_ReturnsBadRequest_WhenInvalidSkill()
        {
            var ctrl = CreateAuthenticatedController(out var userMock, out var svcMock);
            var dto = new CreateSkillLevelDto { SkillId = Guid.NewGuid(), Value = 3, Title = "Intermediate", Description = "Mid level" };

            svcMock.Setup(s => s.CreateSkillLevelAsync(dto, _testUserId))
                .ThrowsAsync(new InvalidOperationException("Skill not found"));

            var res = await ctrl.CreateSkillLevel(dto);
            var bad = Assert.IsType<BadRequestObjectResult>(res);
            Assert.NotNull(bad.Value);
        }

        [Fact]
        public async Task UpdateSkillLevel_ReturnsOk_WhenValid()
        {
            var ctrl = CreateAuthenticatedController(out var userMock, out var svcMock);
            var id = Guid.NewGuid();
            var dto = new UpdateSkillLevelDto { Title = "Updated Level" };
            var expectedResult = new SkillLevelDto { Id = id, SkillId = Guid.NewGuid(), Value = 3, Title = "Updated Level", Description = "Original" };

            svcMock.Setup(s => s.UpdateSkillLevelAsync(id, dto, _testUserId))
                .ReturnsAsync(expectedResult);

            var res = await ctrl.UpdateSkillLevel(id, dto);
            var ok = Assert.IsType<OkObjectResult>(res);
            var result = Assert.IsType<SkillLevelDto>(ok.Value);
            Assert.Equal("Updated Level", result.Title);
        }

        [Fact]
        public async Task UpdateSkillLevel_ReturnsUnauthorized_WhenNotAuthenticated()
        {
            var ctrl = CreateUnauthenticatedController(out var userMock, out var svcMock);
            var dto = new UpdateSkillLevelDto { Title = "Updated" };

            var res = await ctrl.UpdateSkillLevel(Guid.NewGuid(), dto);
            Assert.IsType<UnauthorizedResult>(res);
        }

        [Fact]
        public async Task UpdateSkillLevel_ReturnsBadRequest_WhenNotFound()
        {
            var ctrl = CreateAuthenticatedController(out var userMock, out var svcMock);
            var id = Guid.NewGuid();
            var dto = new UpdateSkillLevelDto { Title = "Updated" };

            svcMock.Setup(s => s.UpdateSkillLevelAsync(id, dto, _testUserId))
                .ThrowsAsync(new InvalidOperationException("Skill level not found"));

            var res = await ctrl.UpdateSkillLevel(id, dto);
            var bad = Assert.IsType<BadRequestObjectResult>(res);
            Assert.NotNull(bad.Value);
        }

        [Fact]
        public async Task DeleteSkillLevel_ReturnsNoContent_WhenValid()
        {
            var ctrl = CreateAuthenticatedController(out var userMock, out var svcMock);
            var id = Guid.NewGuid();

            svcMock.Setup(s => s.DeleteSkillLevelAsync(id, _testUserId))
                .Returns(Task.CompletedTask);

            var res = await ctrl.DeleteSkillLevel(id);
            Assert.IsType<NoContentResult>(res);
        }

        [Fact]
        public async Task DeleteSkillLevel_ReturnsUnauthorized_WhenNotAuthenticated()
        {
            var ctrl = CreateUnauthenticatedController(out var userMock, out var svcMock);

            var res = await ctrl.DeleteSkillLevel(Guid.NewGuid());
            Assert.IsType<UnauthorizedResult>(res);
        }

        [Fact]
        public async Task DeleteSkillLevel_ReturnsBadRequest_WhenHasDependencies()
        {
            var ctrl = CreateAuthenticatedController(out var userMock, out var svcMock);
            var id = Guid.NewGuid();

            svcMock.Setup(s => s.DeleteSkillLevelAsync(id, _testUserId))
                .ThrowsAsync(new InvalidOperationException("Cannot delete skill level with existing mappings"));

            var res = await ctrl.DeleteSkillLevel(id);
            var bad = Assert.IsType<BadRequestObjectResult>(res);
            Assert.NotNull(bad.Value);
        }

        #endregion

        #region Position-Skill Mapping Tests

        [Fact]
        public async Task GetPositionSkills_ReturnsOk_WithMappings()
        {
            var ctrl = CreateControllerWithMockedService(out var mock);
            var positionId = Guid.NewGuid();
            var mappings = new[]
            {
                new PositionSkillMappingDto
                {
                    Id = Guid.NewGuid(),
                    PositionId = positionId,
                    SkillId = Guid.NewGuid(),
                    SkillTitle = "C#",
                    SkillLevelId = Guid.NewGuid(),
                    SkillLevelTitle = "Advanced",
                    CreatedAt = DateTimeOffset.UtcNow
                }
            };

            mock.Setup(s => s.GetPositionSkillMappingsAsync(positionId))
                .ReturnsAsync(mappings);

            var res = await ctrl.GetPositionSkills(positionId);
            var ok = Assert.IsType<OkObjectResult>(res);
            var result = Assert.IsAssignableFrom<System.Collections.Generic.IEnumerable<PositionSkillMappingDto>>(ok.Value);
            Assert.Single(result);
            Assert.Equal("C#", result.First().SkillTitle);
        }

        [Fact]
        public async Task AddSkillToPosition_ReturnsCreated_WhenValid()
        {
            var ctrl = CreateAuthenticatedController(out var userMock, out var svcMock);
            var positionId = Guid.NewGuid();
            var skillId = Guid.NewGuid();
            var skillLevelId = Guid.NewGuid();
            var dto = new CreatePositionSkillMappingDto { SkillId = skillId, SkillLevelId = skillLevelId };
            var expectedResult = new PositionSkillMappingDto
            {
                Id = Guid.NewGuid(),
                PositionId = positionId,
                SkillId = skillId,
                SkillTitle = "C#",
                SkillLevelId = skillLevelId,
                SkillLevelTitle = "Advanced",
                CreatedAt = DateTimeOffset.UtcNow
            };

            svcMock.Setup(s => s.AddSkillToPositionAsync(positionId, dto, _testUserId))
                .ReturnsAsync(expectedResult);

            var res = await ctrl.AddSkillToPosition(positionId, dto);
            var created = Assert.IsType<CreatedAtActionResult>(res);
            var result = Assert.IsType<PositionSkillMappingDto>(created.Value);
            Assert.Equal("C#", result.SkillTitle);
        }

        [Fact]
        public async Task AddSkillToPosition_ReturnsUnauthorized_WhenNotAuthenticated()
        {
            var ctrl = CreateUnauthenticatedController(out var userMock, out var svcMock);
            var dto = new CreatePositionSkillMappingDto { SkillId = Guid.NewGuid(), SkillLevelId = Guid.NewGuid() };

            var res = await ctrl.AddSkillToPosition(Guid.NewGuid(), dto);
            Assert.IsType<UnauthorizedResult>(res);
        }

        [Fact]
        public async Task AddSkillToPosition_ReturnsBadRequest_WhenPositionNotFound()
        {
            var ctrl = CreateAuthenticatedController(out var userMock, out var svcMock);
            var positionId = Guid.NewGuid();
            var dto = new CreatePositionSkillMappingDto { SkillId = Guid.NewGuid(), SkillLevelId = Guid.NewGuid() };

            svcMock.Setup(s => s.AddSkillToPositionAsync(positionId, dto, _testUserId))
                .ThrowsAsync(new InvalidOperationException("Position not found"));

            var res = await ctrl.AddSkillToPosition(positionId, dto);
            var bad = Assert.IsType<BadRequestObjectResult>(res);
            Assert.NotNull(bad.Value);
        }

        [Fact]
        public async Task AddSkillToPosition_ReturnsBadRequest_WhenSkillLevelMismatch()
        {
            var ctrl = CreateAuthenticatedController(out var userMock, out var svcMock);
            var positionId = Guid.NewGuid();
            var dto = new CreatePositionSkillMappingDto { SkillId = Guid.NewGuid(), SkillLevelId = Guid.NewGuid() };

            svcMock.Setup(s => s.AddSkillToPositionAsync(positionId, dto, _testUserId))
                .ThrowsAsync(new InvalidOperationException("Skill level does not belong to the specified skill"));

            var res = await ctrl.AddSkillToPosition(positionId, dto);
            var bad = Assert.IsType<BadRequestObjectResult>(res);
            Assert.NotNull(bad.Value);
        }

        [Fact]
        public async Task AddSkillToPosition_ReturnsBadRequest_WhenDuplicateMapping()
        {
            var ctrl = CreateAuthenticatedController(out var userMock, out var svcMock);
            var positionId = Guid.NewGuid();
            var dto = new CreatePositionSkillMappingDto { SkillId = Guid.NewGuid(), SkillLevelId = Guid.NewGuid() };

            svcMock.Setup(s => s.AddSkillToPositionAsync(positionId, dto, _testUserId))
                .ThrowsAsync(new InvalidOperationException("This skill is already mapped to this position"));

            var res = await ctrl.AddSkillToPosition(positionId, dto);
            var bad = Assert.IsType<BadRequestObjectResult>(res);
            Assert.NotNull(bad.Value);
        }

        [Fact]
        public async Task RemoveSkillFromPosition_ReturnsNoContent_WhenValid()
        {
            var ctrl = CreateAuthenticatedController(out var userMock, out var svcMock);
            var positionId = Guid.NewGuid();
            var skillId = Guid.NewGuid();

            svcMock.Setup(s => s.RemoveSkillFromPositionAsync(positionId, skillId, _testUserId))
                .Returns(Task.CompletedTask);

            var res = await ctrl.RemoveSkillFromPosition(positionId, skillId);
            Assert.IsType<NoContentResult>(res);
        }

        [Fact]
        public async Task RemoveSkillFromPosition_ReturnsUnauthorized_WhenNotAuthenticated()
        {
            var ctrl = CreateUnauthenticatedController(out var userMock, out var svcMock);

            var res = await ctrl.RemoveSkillFromPosition(Guid.NewGuid(), Guid.NewGuid());
            Assert.IsType<UnauthorizedResult>(res);
        }

        [Fact]
        public async Task RemoveSkillFromPosition_ReturnsBadRequest_WhenMappingNotFound()
        {
            var ctrl = CreateAuthenticatedController(out var userMock, out var svcMock);
            var positionId = Guid.NewGuid();
            var skillId = Guid.NewGuid();

            svcMock.Setup(s => s.RemoveSkillFromPositionAsync(positionId, skillId, _testUserId))
                .ThrowsAsync(new InvalidOperationException("Position-skill mapping not found"));

            var res = await ctrl.RemoveSkillFromPosition(positionId, skillId);
            var bad = Assert.IsType<BadRequestObjectResult>(res);
            Assert.NotNull(bad.Value);
        }

        #endregion
    }
}
