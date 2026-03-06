#nullable enable
using System;
using Xunit;
using FluentValidation.TestHelper;
using CPR.Application.DTOs.Taxonomy;
using CPR.Application.Validators.Taxonomy;

namespace CPR.UnitTests.Validators
{
    // ==================== CareerPath Validators ====================

    public class CreateCareerPathDtoValidatorTests
    {
        private readonly CreateCareerPathDtoValidator _v = new();

        [Fact]
        public void ValidInput_Passes()
        {
            var result = _v.TestValidate(new CreateCareerPathDto { Title = "Engineering" });
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void EmptyTitle_Fails()
        {
            var result = _v.TestValidate(new CreateCareerPathDto { Title = "" });
            result.ShouldHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Title200Chars_Passes()
        {
            var result = _v.TestValidate(new CreateCareerPathDto { Title = new string('a', 200) });
            result.ShouldNotHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Title201Chars_Fails()
        {
            var result = _v.TestValidate(new CreateCareerPathDto { Title = new string('a', 201) });
            result.ShouldHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Description1000Chars_Passes()
        {
            var result = _v.TestValidate(new CreateCareerPathDto { Title = "T", Description = new string('x', 1000) });
            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Description1001Chars_Fails()
        {
            var result = _v.TestValidate(new CreateCareerPathDto { Title = "T", Description = new string('x', 1001) });
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void NullDescription_Passes()
        {
            var result = _v.TestValidate(new CreateCareerPathDto { Title = "T", Description = null });
            result.ShouldNotHaveAnyValidationErrors();
        }
    }

    public class UpdateCareerPathDtoValidatorTests
    {
        private readonly UpdateCareerPathDtoValidator _v = new();

        [Fact]
        public void AllNull_Passes()
        {
            var result = _v.TestValidate(new UpdateCareerPathDto());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void EmptyTitleString_Fails()
        {
            var result = _v.TestValidate(new UpdateCareerPathDto { Title = "" });
            result.ShouldHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Title200Chars_Passes()
        {
            var result = _v.TestValidate(new UpdateCareerPathDto { Title = new string('a', 200) });
            result.ShouldNotHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Title201Chars_Fails()
        {
            var result = _v.TestValidate(new UpdateCareerPathDto { Title = new string('a', 201) });
            result.ShouldHaveValidationErrorFor(x => x.Title);
        }
    }

    // ==================== CareerTrack Validators ====================

    public class CreateCareerTrackDtoValidatorTests
    {
        private readonly CreateCareerTrackDtoValidator _v = new();

        [Fact]
        public void ValidInput_Passes()
        {
            var result = _v.TestValidate(new CreateCareerTrackDto { Title = "Backend", CareerPathId = Guid.NewGuid() });
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void EmptyTitle_Fails()
        {
            var result = _v.TestValidate(new CreateCareerTrackDto { Title = "", CareerPathId = Guid.NewGuid() });
            result.ShouldHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Title200Chars_Passes()
        {
            var result = _v.TestValidate(new CreateCareerTrackDto { Title = new string('a', 200), CareerPathId = Guid.NewGuid() });
            result.ShouldNotHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Title201Chars_Fails()
        {
            var result = _v.TestValidate(new CreateCareerTrackDto { Title = new string('a', 201), CareerPathId = Guid.NewGuid() });
            result.ShouldHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void EmptyCareerPathId_Fails()
        {
            var result = _v.TestValidate(new CreateCareerTrackDto { Title = "Backend", CareerPathId = Guid.Empty });
            result.ShouldHaveValidationErrorFor(x => x.CareerPathId);
        }

        [Fact]
        public void Description1000Chars_Passes()
        {
            var result = _v.TestValidate(new CreateCareerTrackDto
            {
                Title = "T", CareerPathId = Guid.NewGuid(), Description = new string('x', 1000)
            });
            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Description1001Chars_Fails()
        {
            var result = _v.TestValidate(new CreateCareerTrackDto
            {
                Title = "T", CareerPathId = Guid.NewGuid(), Description = new string('x', 1001)
            });
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }
    }

    // ==================== Position Validators ====================

    public class CreatePositionDtoValidatorTests
    {
        private readonly CreatePositionDtoValidator _v = new();

        [Fact]
        public void ValidInput_Passes()
        {
            var result = _v.TestValidate(new CreatePositionDto { Title = "Senior Dev", CareerTrackId = Guid.NewGuid() });
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void EmptyTitle_Fails()
        {
            var result = _v.TestValidate(new CreatePositionDto { Title = "", CareerTrackId = Guid.NewGuid() });
            result.ShouldHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Title200Chars_Passes()
        {
            var result = _v.TestValidate(new CreatePositionDto { Title = new string('a', 200), CareerTrackId = Guid.NewGuid() });
            result.ShouldNotHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Title201Chars_Fails()
        {
            var result = _v.TestValidate(new CreatePositionDto { Title = new string('a', 201), CareerTrackId = Guid.NewGuid() });
            result.ShouldHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void EmptyCareerTrackId_Fails()
        {
            var result = _v.TestValidate(new CreatePositionDto { Title = "Dev", CareerTrackId = Guid.Empty });
            result.ShouldHaveValidationErrorFor(x => x.CareerTrackId);
        }

        [Fact]
        public void Description2000Chars_Passes()
        {
            var result = _v.TestValidate(new CreatePositionDto
            {
                Title = "T", CareerTrackId = Guid.NewGuid(), Description = new string('x', 2000)
            });
            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Description2001Chars_Fails()
        {
            var result = _v.TestValidate(new CreatePositionDto
            {
                Title = "T", CareerTrackId = Guid.NewGuid(), Description = new string('x', 2001)
            });
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Expectations2000Chars_Passes()
        {
            var result = _v.TestValidate(new CreatePositionDto
            {
                Title = "T", CareerTrackId = Guid.NewGuid(), Expectations = new string('x', 2000)
            });
            result.ShouldNotHaveValidationErrorFor(x => x.Expectations);
        }

        [Fact]
        public void Expectations2001Chars_Fails()
        {
            var result = _v.TestValidate(new CreatePositionDto
            {
                Title = "T", CareerTrackId = Guid.NewGuid(), Expectations = new string('x', 2001)
            });
            result.ShouldHaveValidationErrorFor(x => x.Expectations);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(100)]
        public void NonNegativeSortOrder_Passes(int value)
        {
            var result = _v.TestValidate(new CreatePositionDto { Title = "T", CareerTrackId = Guid.NewGuid(), SortOrder = value });
            result.ShouldNotHaveValidationErrorFor(x => x.SortOrder);
        }

        [Fact]
        public void NegativeSortOrder_Fails()
        {
            var result = _v.TestValidate(new CreatePositionDto { Title = "T", CareerTrackId = Guid.NewGuid(), SortOrder = -1 });
            result.ShouldHaveValidationErrorFor(x => x.SortOrder);
        }
    }

    // ==================== SkillCategory Validators ====================

    public class CreateSkillCategoryDtoValidatorTests
    {
        private readonly CreateSkillCategoryDtoValidator _v = new();

        [Fact]
        public void ValidInput_Passes()
        {
            var result = _v.TestValidate(new CreateSkillCategoryDto { Title = "Technical" });
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void EmptyTitle_Fails()
        {
            var result = _v.TestValidate(new CreateSkillCategoryDto { Title = "" });
            result.ShouldHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Title200Chars_Passes()
        {
            var result = _v.TestValidate(new CreateSkillCategoryDto { Title = new string('a', 200) });
            result.ShouldNotHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Title201Chars_Fails()
        {
            var result = _v.TestValidate(new CreateSkillCategoryDto { Title = new string('a', 201) });
            result.ShouldHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Description1000Chars_Passes()
        {
            var result = _v.TestValidate(new CreateSkillCategoryDto { Title = "T", Description = new string('x', 1000) });
            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Description1001Chars_Fails()
        {
            var result = _v.TestValidate(new CreateSkillCategoryDto { Title = "T", Description = new string('x', 1001) });
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }
    }

    // ==================== Skill Validators ====================

    public class CreateSkillDtoValidatorTests
    {
        private readonly CreateSkillDtoValidator _v = new();

        [Fact]
        public void ValidInput_Passes()
        {
            var result = _v.TestValidate(new CreateSkillDto { Title = "C#", CategoryId = Guid.NewGuid() });
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void EmptyTitle_Fails()
        {
            var result = _v.TestValidate(new CreateSkillDto { Title = "", CategoryId = Guid.NewGuid() });
            result.ShouldHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Title200Chars_Passes()
        {
            var result = _v.TestValidate(new CreateSkillDto { Title = new string('a', 200), CategoryId = Guid.NewGuid() });
            result.ShouldNotHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Title201Chars_Fails()
        {
            var result = _v.TestValidate(new CreateSkillDto { Title = new string('a', 201), CategoryId = Guid.NewGuid() });
            result.ShouldHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void EmptyCategoryId_Fails()
        {
            var result = _v.TestValidate(new CreateSkillDto { Title = "C#", CategoryId = Guid.Empty });
            result.ShouldHaveValidationErrorFor(x => x.CategoryId);
        }

        [Fact]
        public void Description1000Chars_Passes()
        {
            var result = _v.TestValidate(new CreateSkillDto { Title = "T", CategoryId = Guid.NewGuid(), Description = new string('x', 1000) });
            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Description1001Chars_Fails()
        {
            var result = _v.TestValidate(new CreateSkillDto { Title = "T", CategoryId = Guid.NewGuid(), Description = new string('x', 1001) });
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }
    }

    // ==================== SkillLevel Validators ====================

    public class AddSkillLevelDtoValidatorTests
    {
        private readonly AddSkillLevelDtoValidator _v = new();

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(5)]
        public void ValidValues_Pass(int value)
        {
            var result = _v.TestValidate(new AddSkillLevelDto { Value = value, Title = "Level" });
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(6)]
        [InlineData(-1)]
        public void OutOfRangeValue_Fails(int value)
        {
            var result = _v.TestValidate(new AddSkillLevelDto { Value = value, Title = "Level" });
            result.ShouldHaveValidationErrorFor(x => x.Value);
        }

        [Fact]
        public void EmptyTitle_Fails()
        {
            var result = _v.TestValidate(new AddSkillLevelDto { Value = 3, Title = "" });
            result.ShouldHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Title100Chars_Passes()
        {
            var result = _v.TestValidate(new AddSkillLevelDto { Value = 3, Title = new string('a', 100) });
            result.ShouldNotHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Title101Chars_Fails()
        {
            var result = _v.TestValidate(new AddSkillLevelDto { Value = 3, Title = new string('a', 101) });
            result.ShouldHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Description500Chars_Passes()
        {
            var result = _v.TestValidate(new AddSkillLevelDto { Value = 1, Title = "T", Description = new string('x', 500) });
            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Description501Chars_Fails()
        {
            var result = _v.TestValidate(new AddSkillLevelDto { Value = 1, Title = "T", Description = new string('x', 501) });
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }
    }

    public class UpdateSkillLevelDtoValidatorTests
    {
        private readonly UpdateSkillLevelDtoValidator _v = new();

        [Fact]
        public void AllNull_Passes()
        {
            var result = _v.TestValidate(new UpdateSkillLevelDto());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        public void ValidValue_Passes(int value)
        {
            var result = _v.TestValidate(new UpdateSkillLevelDto { Value = value });
            result.ShouldNotHaveValidationErrorFor(x => x.Value);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(6)]
        public void OutOfRangeValue_Fails(int value)
        {
            var result = _v.TestValidate(new UpdateSkillLevelDto { Value = value });
            result.ShouldHaveValidationErrorFor(x => x.Value);
        }

        [Fact]
        public void EmptyTitleString_Fails()
        {
            var result = _v.TestValidate(new UpdateSkillLevelDto { Title = "" });
            result.ShouldHaveValidationErrorFor(x => x.Title);
        }
    }

    // ==================== PositionSkill Validators ====================

    public class AddPositionSkillDtoValidatorTests
    {
        private readonly AddPositionSkillDtoValidator _v = new();

        [Fact]
        public void ValidInput_Passes()
        {
            var result = _v.TestValidate(new AddPositionSkillDto
            {
                SkillId = Guid.NewGuid(),
                SkillLevelId = Guid.NewGuid(),
                IsMandatory = true
            });
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void EmptySkillId_Fails()
        {
            var result = _v.TestValidate(new AddPositionSkillDto
            {
                SkillId = Guid.Empty,
                SkillLevelId = Guid.NewGuid()
            });
            result.ShouldHaveValidationErrorFor(x => x.SkillId);
        }

        [Fact]
        public void EmptySkillLevelId_Fails()
        {
            var result = _v.TestValidate(new AddPositionSkillDto
            {
                SkillId = Guid.NewGuid(),
                SkillLevelId = Guid.Empty
            });
            result.ShouldHaveValidationErrorFor(x => x.SkillLevelId);
        }

        [Fact]
        public void ZeroWeight_Fails()
        {
            var result = _v.TestValidate(new AddPositionSkillDto
            {
                SkillId = Guid.NewGuid(),
                SkillLevelId = Guid.NewGuid(),
                Weight = 0m
            });
            result.ShouldHaveValidationErrorFor(x => x.Weight);
        }

        [Fact]
        public void PositiveWeight_Passes()
        {
            var result = _v.TestValidate(new AddPositionSkillDto
            {
                SkillId = Guid.NewGuid(),
                SkillLevelId = Guid.NewGuid(),
                Weight = 1.5m
            });
            result.ShouldNotHaveValidationErrorFor(x => x.Weight);
        }

        [Fact]
        public void Rationale500Chars_Passes()
        {
            var result = _v.TestValidate(new AddPositionSkillDto
            {
                SkillId = Guid.NewGuid(),
                SkillLevelId = Guid.NewGuid(),
                Rationale = new string('x', 500)
            });
            result.ShouldNotHaveValidationErrorFor(x => x.Rationale);
        }

        [Fact]
        public void Rationale501Chars_Fails()
        {
            var result = _v.TestValidate(new AddPositionSkillDto
            {
                SkillId = Guid.NewGuid(),
                SkillLevelId = Guid.NewGuid(),
                Rationale = new string('x', 501)
            });
            result.ShouldHaveValidationErrorFor(x => x.Rationale);
        }
    }

    public class UpdatePositionSkillDtoValidatorTests
    {
        private readonly UpdatePositionSkillDtoValidator _v = new();

        [Fact]
        public void AllNull_Passes()
        {
            var result = _v.TestValidate(new UpdatePositionSkillDto());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void ZeroWeight_Fails()
        {
            var result = _v.TestValidate(new UpdatePositionSkillDto { Weight = 0m });
            result.ShouldHaveValidationErrorFor(x => x.Weight);
        }

        [Fact]
        public void PositiveWeight_Passes()
        {
            var result = _v.TestValidate(new UpdatePositionSkillDto { Weight = 2m });
            result.ShouldNotHaveValidationErrorFor(x => x.Weight);
        }

        [Fact]
        public void Rationale501Chars_Fails()
        {
            var result = _v.TestValidate(new UpdatePositionSkillDto { Rationale = new string('x', 501) });
            result.ShouldHaveValidationErrorFor(x => x.Rationale);
        }
    }
}
