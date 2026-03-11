using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xunit;
using CPR.Application.DTOs.SkillAssessment;
using CPR.Application.DTOs.Taxonomy;

namespace CPR.UnitTests.Validators
{
    /// <summary>
    /// Tests for DataAnnotation validation on SkillAssessment DTOs.
    /// Mirrors what ASP.NET Core [ApiController] does on model binding.
    /// </summary>
    public class SkillAssessmentValidatorTests
    {
        private static bool IsValid(object dto, out ICollection<ValidationResult> results)
        {
            var ctx = new ValidationContext(dto, serviceProvider: null, items: null);
            results = new List<ValidationResult>();
            return Validator.TryValidateObject(dto, ctx, results, validateAllProperties: true);
        }

        // ---------- UpsertSkillAssessmentDto ----------

        [Fact]
        public void UpsertSkillAssessmentDto_ValidInput_Passes()
        {
            var dto = new UpsertSkillAssessmentDto { SelfAssessmentValue = 3.0m, Notes = "Some notes" };
            Assert.True(IsValid(dto, out _));
        }

        [Fact]
        public void UpsertSkillAssessmentDto_NullNotes_Passes()
        {
            var dto = new UpsertSkillAssessmentDto { SelfAssessmentValue = 2.5m, Notes = null };
            Assert.True(IsValid(dto, out _));
        }

        [Fact]
        public void UpsertSkillAssessmentDto_Notes1000Chars_Passes()
        {
            var dto = new UpsertSkillAssessmentDto { SelfAssessmentValue = 1.0m, Notes = new string('x', 1000) };
            Assert.True(IsValid(dto, out _));
        }

        [Fact]
        public void UpsertSkillAssessmentDto_Notes1001Chars_Fails()
        {
            var dto = new UpsertSkillAssessmentDto { SelfAssessmentValue = 1.0m, Notes = new string('x', 1001) };
            Assert.False(IsValid(dto, out _));
        }

        [Fact]
        public void UpsertSkillAssessmentDto_ZeroValue_Fails()
        {
            var dto = new UpsertSkillAssessmentDto { SelfAssessmentValue = 0m };
            Assert.False(IsValid(dto, out _));
        }

        [Fact]
        public void UpsertSkillAssessmentDto_NegativeValue_Fails()
        {
            var dto = new UpsertSkillAssessmentDto { SelfAssessmentValue = -1.0m };
            Assert.False(IsValid(dto, out _));
        }

        // ---------- UpsertManagerAssessmentDto ----------

        [Fact]
        public void UpsertManagerAssessmentDto_ValidInput_Passes()
        {
            var dto = new UpsertManagerAssessmentDto { ManagerAssessmentValue = 4.0m };
            Assert.True(IsValid(dto, out _));
        }

        [Fact]
        public void UpsertManagerAssessmentDto_ZeroValue_Fails()
        {
            var dto = new UpsertManagerAssessmentDto { ManagerAssessmentValue = 0m };
            Assert.False(IsValid(dto, out _));
        }

        [Fact]
        public void UpsertManagerAssessmentDto_NegativeValue_Fails()
        {
            var dto = new UpsertManagerAssessmentDto { ManagerAssessmentValue = -0.5m };
            Assert.False(IsValid(dto, out _));
        }

        // ---------- LinkEvidenceDto ----------

        [Fact]
        public void LinkEvidenceDto_ValidInput_Passes()
        {
            var dto = new LinkEvidenceDto { FeedbackId = Guid.NewGuid() };
            Assert.True(IsValid(dto, out _));
        }

        // ---------- AC-034/AC-035/AC-036: weight removed from position_to_skill ----------

        [Fact]
        public void AddPositionSkillDto_DoesNotHave_WeightProperty()
        {
            // AC-034/AC-035: weight column removed; DTO must not expose it
            var type = typeof(AddPositionSkillDto);
            Assert.Null(type.GetProperty("Weight"));
        }

        [Fact]
        public void UpdatePositionSkillDto_DoesNotHave_WeightProperty()
        {
            // AC-036: PATCH silently ignores weight — DTO doesn't have Weight property
            var type = typeof(UpdatePositionSkillDto);
            Assert.Null(type.GetProperty("Weight"));
        }

        [Fact]
        public void PositionSkillRequirementDto_DoesNotHave_WeightProperty()
        {
            // AC-035: GET position-skill response does not include weight
            var type = typeof(PositionSkillRequirementDto);
            Assert.Null(type.GetProperty("Weight"));
        }
    }
}
