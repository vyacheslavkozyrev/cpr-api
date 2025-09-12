using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CPR.Api.Controllers;
using CPR.Application.Contracts;
using CPR.Application.Services;
using CPR.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace CPR.UnitTests
{
    public class GoalsControllerValidationTests
    {
        private GoalsController CreateController(string employeeId)
        {
            var userService = new Mock<IUserService>();
            userService.Setup(s => s.GetCurrentUserProfile(It.IsAny<ClaimsPrincipal>()))
                .Returns(new Api.Models.UserProfile { EmployeeId = employeeId });

            var goalService = new Mock<IGoalService>();
            goalService.Setup(s => s.CreateGoalAsync(It.IsAny<Guid>(), It.IsAny<CreateGoalDto>()))
                .ReturnsAsync((Guid owner, CreateGoalDto dto) => new Application.Contracts.GoalDto { Id = Guid.NewGuid(), Title = dto.Title });

            var controller = new GoalsController(userService.Object, goalService.Object);
            // set a valid HttpContext with a ClaimsPrincipal
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
            };

            return controller;
        }

        [Fact]
        public async Task Create_InvalidPriority_ModelStateError()
        {
            var ctrl = CreateController(Guid.NewGuid().ToString());
            var dto = new CreateGoalDto { Title = "x", Priority = 1000 };

            // Force model validation
            ctrl.TryValidateModel(dto);
            Assert.False(ctrl.ModelState.IsValid);

            var res = await ctrl.Create(dto);
            Assert.IsType<BadRequestObjectResult>(res);
        }

        [Fact]
        public async Task Create_InvalidVisibility_ModelStateError()
        {
            var ctrl = CreateController(Guid.NewGuid().ToString());
            var dto = new CreateGoalDto { Title = "x", Visibility = "everyone" };

            ctrl.TryValidateModel(dto);
            Assert.False(ctrl.ModelState.IsValid);

            var res = await ctrl.Create(dto);
            Assert.IsType<BadRequestObjectResult>(res);
        }

        [Fact]
        public async Task Create_InvalidEmployeeIdInDto_ModelStateError()
        {
            var ctrl = CreateController(Guid.NewGuid().ToString());
            // EmployeeId property expects a GUID; we'll provide invalid string via model binder path isn't available here
            // So we verify that setting EmployeeId to an invalid Guid cannot be done (it's Guid?), but we can simulate by adding model error
            var dto = new CreateGoalDto { Title = "x" };
            ctrl.ModelState.AddModelError("EmployeeId", "EmployeeId must be a valid GUID");

            var res = await ctrl.Create(dto);
            Assert.IsType<BadRequestObjectResult>(res);
        }
    }
}
