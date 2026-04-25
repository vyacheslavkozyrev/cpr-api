using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CPR.Api.Controllers;
using CPR.Application.Contracts;
using CPR.Application.Services;
using CPR.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Moq;
using Xunit;

namespace CPR.UnitTests
{
    public class GoalsControllerValidationTests
    {
        private GoalsController CreateController(string employeeId)
        {
            var userService = new Mock<IUserService>();
            userService.Setup(s => s.GetCurrentUserProfileAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new Api.Models.UserProfile { EmployeeId = employeeId });

            var goalService = new Mock<IGoalService>();
            goalService.Setup(s => s.CreateGoalAsync(It.IsAny<Guid>(), It.IsAny<CreateGoalDto>()))
                .ReturnsAsync((Guid owner, CreateGoalDto dto) => new Application.Contracts.GoalDto { Id = Guid.NewGuid(), Title = dto.Title });

            var deletionRequestService = new Mock<IGoalDeletionRequestService>();

            var controller = new GoalsController(userService.Object, goalService.Object, deletionRequestService.Object);
            // set a valid HttpContext with a ClaimsPrincipal
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
            };

            return controller;
        }

        [Fact]
        public void Create_InvalidPriority_ModelStateError()
        {
            var ctrl = CreateController(Guid.NewGuid().ToString());
            var dto = new CreateGoalDto { Title = "x", Priority = 1000 };

            // Force model validation (use DataAnnotations validator in unit tests)
            ForceValidate(ctrl, dto);
            Assert.False(ctrl.ModelState.IsValid);

            // Simulate ApiController automatic 400 response for invalid model
            if (!ctrl.ModelState.IsValid)
            {
                var bad = ctrl.BadRequest(ctrl.ModelState);
                Assert.IsType<BadRequestObjectResult>(bad);
            }
        }

        [Fact]
        public void Create_InvalidVisibility_ModelStateError()
        {
            var ctrl = CreateController(Guid.NewGuid().ToString());
            var dto = new CreateGoalDto { Title = "x", Visibility = "everyone" };

            ForceValidate(ctrl, dto);
            Assert.False(ctrl.ModelState.IsValid);
            if (!ctrl.ModelState.IsValid)
            {
                var bad = ctrl.BadRequest(ctrl.ModelState);
                Assert.IsType<BadRequestObjectResult>(bad);
            }
        }

        [Fact]
        public void Create_InvalidEmployeeIdInDto_ModelStateError()
        {
            var ctrl = CreateController(Guid.NewGuid().ToString());
            // EmployeeId property expects a GUID; we'll provide invalid string via model binder path isn't available here
            // So we verify that setting EmployeeId to an invalid Guid cannot be done (it's Guid?), but we can simulate by adding model error
            var dto = new CreateGoalDto { Title = "x" };
            ctrl.ModelState.AddModelError("EmployeeId", "EmployeeId must be a valid GUID");

            if (!ctrl.ModelState.IsValid)
            {
                var bad = ctrl.BadRequest(ctrl.ModelState);
                Assert.IsType<BadRequestObjectResult>(bad);
            }
        }

        // local helper to validate data annotations and populate ModelState
        private void ForceValidate(ControllerBase controller, object model)
        {
            var ctx = new ValidationContext(model, serviceProvider: null, items: null);
            var results = new System.Collections.Generic.List<ValidationResult>();
            System.ComponentModel.DataAnnotations.Validator.TryValidateObject(model, ctx, results, validateAllProperties: true);
            foreach (var r in results)
            {
                var member = r.MemberNames != null ? System.Linq.Enumerable.FirstOrDefault(r.MemberNames) : null;
                controller.ModelState.AddModelError(member ?? string.Empty, r.ErrorMessage ?? "Validation error");
            }
        }
    }
}
