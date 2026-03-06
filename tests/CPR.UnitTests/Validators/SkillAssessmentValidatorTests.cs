using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xunit;
using CPR.Application.DTOs.SkillAssessment;

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
            var dto = new UpsertSkillAssessmentDto { SkillLevelId = Guid.NewGuid(), Notes = "Some notes" };
            Assert.True(IsValid(dto, out _));
        }

        [Fact]
        public void UpsertSkillAssessmentDto_NullNotes_Passes()
        {
            var dto = new UpsertSkillAssessmentDto { SkillLevelId = Guid.NewGuid(), Notes = null };
            Assert.True(IsValid(dto, out _));
        }

        [Fact]
        public void UpsertSkillAssessmentDto_Notes1000Chars_Passes()
        {
            var dto = new UpsertSkillAssessmentDto { SkillLevelId = Guid.NewGuid(), Notes = new string('x', 1000) };
            Assert.True(IsValid(dto, out _));
        }

        [Fact]
        public void UpsertSkillAssessmentDto_Notes1001Chars_Fails()
        {
            var dto = new UpsertSkillAssessmentDto { SkillLevelId = Guid.NewGuid(), Notes = new string('x', 1001) };
            Assert.False(IsValid(dto, out _));
        }

        // ---------- UpsertSkillTargetDto ----------

        [Fact]
        public void UpsertSkillTargetDto_ValidInput_Passes()
        {
            var dto = new UpsertSkillTargetDto { SkillLevelId = Guid.NewGuid() };
            Assert.True(IsValid(dto, out _));
        }

        // ---------- LinkEvidenceDto ----------

        [Fact]
        public void LinkEvidenceDto_ValidInput_Passes()
        {
            var dto = new LinkEvidenceDto { FeedbackId = Guid.NewGuid() };
            Assert.True(IsValid(dto, out _));
        }
    }
}
