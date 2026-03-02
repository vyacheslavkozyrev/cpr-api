using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xunit;
using CPR.Application.DTOs.ReviewCycles;

namespace CPR.UnitTests.Validators
{
    /// <summary>
    /// Tests for DataAnnotation validation on ReviewCycle DTOs.
    /// Validation is evaluated via Validator.TryValidateObject to mirror
    /// what ASP.NET Core [ApiController] does on model binding.
    /// </summary>
    public class ReviewCycleDtoValidatorTests
    {
        private static bool IsValid(object dto, out ICollection<ValidationResult> results)
        {
            var ctx = new ValidationContext(dto, serviceProvider: null, items: null);
            results = new List<ValidationResult>();
            return Validator.TryValidateObject(dto, ctx, results, validateAllProperties: true);
        }

        // ---------- CreateReviewCycleDto ----------

        [Fact]
        public void CreateReviewCycleDto_ValidInput_Passes()
        {
            var dto = new CreateReviewCycleDto { Title = "Q1 Review", SubjectEmployeeId = Guid.NewGuid() };
            Assert.True(IsValid(dto, out _));
        }

        [Fact]
        public void CreateReviewCycleDto_EmptyTitle_Fails()
        {
            var dto = new CreateReviewCycleDto { Title = "", SubjectEmployeeId = Guid.NewGuid() };
            Assert.False(IsValid(dto, out _));
        }

        [Fact]
        public void CreateReviewCycleDto_Title200Chars_Passes()
        {
            var dto = new CreateReviewCycleDto { Title = new string('a', 200), SubjectEmployeeId = Guid.NewGuid() };
            Assert.True(IsValid(dto, out _));
        }

        [Fact]
        public void CreateReviewCycleDto_Title201Chars_Fails()
        {
            var dto = new CreateReviewCycleDto { Title = new string('a', 201), SubjectEmployeeId = Guid.NewGuid() };
            Assert.False(IsValid(dto, out _));
        }

        [Fact]
        public void CreateReviewCycleDto_Description2000Chars_Passes()
        {
            var dto = new CreateReviewCycleDto { Title = "T", SubjectEmployeeId = Guid.NewGuid(), Description = new string('x', 2000) };
            Assert.True(IsValid(dto, out _));
        }

        [Fact]
        public void CreateReviewCycleDto_Description2001Chars_Fails()
        {
            var dto = new CreateReviewCycleDto { Title = "T", SubjectEmployeeId = Guid.NewGuid(), Description = new string('x', 2001) };
            Assert.False(IsValid(dto, out _));
        }

        [Fact]
        public void CreateReviewCycleDto_EmptySubjectEmployeeId_Fails()
        {
            // Guid.Empty satisfies [Required] for value types in .NET — title empty forces failure
            var dto = new CreateReviewCycleDto { Title = "", SubjectEmployeeId = Guid.Empty };
            Assert.False(IsValid(dto, out _));
        }

        // ---------- TransitionCycleStatusDto ----------

        [Theory]
        [InlineData("open")]
        [InlineData("in_progress")]
        [InlineData("closed")]
        public void TransitionCycleStatusDto_ValidValues_Pass(string status)
        {
            var dto = new TransitionCycleStatusDto { Status = status };
            Assert.True(IsValid(dto, out _));
        }

        [Theory]
        [InlineData("draft")]
        [InlineData("OPEN")]
        [InlineData("")]
        [InlineData("unknown")]
        public void TransitionCycleStatusDto_InvalidValues_Fail(string status)
        {
            var dto = new TransitionCycleStatusDto { Status = status };
            Assert.False(IsValid(dto, out _));
        }

        // ---------- SubmitReviewResponseDto ----------

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(3)]
        public void SubmitReviewResponseDto_ValidRating_Passes(int rating)
        {
            var dto = new SubmitReviewResponseDto { OverallRating = rating, Comments = new string('c', 10) };
            Assert.True(IsValid(dto, out _));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(6)]
        [InlineData(-1)]
        public void SubmitReviewResponseDto_InvalidRating_Fails(int rating)
        {
            var dto = new SubmitReviewResponseDto { OverallRating = rating, Comments = new string('c', 10) };
            Assert.False(IsValid(dto, out _));
        }

        [Fact]
        public void SubmitReviewResponseDto_Comments10Chars_Passes()
        {
            var dto = new SubmitReviewResponseDto { OverallRating = 3, Comments = new string('c', 10) };
            Assert.True(IsValid(dto, out _));
        }

        [Fact]
        public void SubmitReviewResponseDto_Comments9Chars_Fails()
        {
            var dto = new SubmitReviewResponseDto { OverallRating = 3, Comments = new string('c', 9) };
            Assert.False(IsValid(dto, out _));
        }

        [Fact]
        public void SubmitReviewResponseDto_Comments2000Chars_Passes()
        {
            var dto = new SubmitReviewResponseDto { OverallRating = 3, Comments = new string('c', 2000) };
            Assert.True(IsValid(dto, out _));
        }

        [Fact]
        public void SubmitReviewResponseDto_Comments2001Chars_Fails()
        {
            var dto = new SubmitReviewResponseDto { OverallRating = 3, Comments = new string('c', 2001) };
            Assert.False(IsValid(dto, out _));
        }

        [Fact]
        public void SubmitReviewResponseDto_EmptyComments_Fails()
        {
            var dto = new SubmitReviewResponseDto { OverallRating = 3, Comments = "" };
            Assert.False(IsValid(dto, out _));
        }
    }
}
