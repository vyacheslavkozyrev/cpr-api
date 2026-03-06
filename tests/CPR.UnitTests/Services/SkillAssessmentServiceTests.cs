using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using CPR.Application.DTOs.SkillAssessment;
using CPR.Application.Repositories;
using CPR.Domain.Entities;
using CPR.Infrastructure.Data;
using CPR.Infrastructure.Services;

namespace CPR.UnitTests.Services
{
    public class SkillAssessmentServiceTests : IDisposable
    {
        private readonly CprDbContext _db;
        private readonly Mock<ISkillAssessmentRepository> _repoMock;
        private readonly SkillAssessmentService _service;

        private static readonly Guid EmployeeId   = Guid.Parse("aa000000-0000-0000-0000-000000000001");
        private static readonly Guid PositionId   = Guid.Parse("bb000000-0000-0000-0000-000000000001");
        private static readonly Guid SkillId      = Guid.Parse("cc000000-0000-0000-0000-000000000001");
        private static readonly Guid Level2Id     = Guid.Parse("dd000000-0000-0000-0000-000000000002");
        private static readonly Guid Level3Id     = Guid.Parse("dd000000-0000-0000-0000-000000000003");
        private static readonly Guid Level4Id     = Guid.Parse("dd000000-0000-0000-0000-000000000004");
        private static readonly Guid FeedbackId   = Guid.Parse("ee000000-0000-0000-0000-000000000001");
        private static readonly Guid AssessmentId = Guid.Parse("ff000000-0000-0000-0000-000000000001");

        public SkillAssessmentServiceTests()
        {
            var options = new DbContextOptionsBuilder<CprDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new CprDbContext(options);
            _repoMock = new Mock<ISkillAssessmentRepository>();
            _service = new SkillAssessmentService(_repoMock.Object, _db);
        }

        public void Dispose() => _db.Dispose();

        // ---------- Helpers ----------

        private static Employee MakeEmployee() => new Employee
        {
            Id = EmployeeId,
            UserId = Guid.NewGuid(),
            PositionId = PositionId,
            IsDeleted = false
        };

        private static PositionSkillRow MakePositionSkillRow() => new PositionSkillRow
        {
            PositionToSkillId = Guid.NewGuid(),
            SkillId = SkillId,
            SkillTitle = "TypeScript",
            CategoryId = Guid.NewGuid(),
            CategoryTitle = "Technical",
            RequiredLevelId = Level3Id,
            RequiredLevelTitle = "Advanced",
            RequiredLevelValue = 3
        };

        private async Task SeedSkillLevels()
        {
            _db.SkillLevels.AddRange(
                new SkillLevel { Id = Level2Id, Title = "Intermediate", Value = 2, SkillId = SkillId, IsDeleted = false },
                new SkillLevel { Id = Level3Id, Title = "Advanced",     Value = 3, SkillId = SkillId, IsDeleted = false },
                new SkillLevel { Id = Level4Id, Title = "Expert",       Value = 4, SkillId = SkillId, IsDeleted = false }
            );
            await _db.SaveChangesAsync();
        }

        // ---------- UpsertCurrentLevelAsync — happy path ----------

        [Fact]
        public async Task UpsertCurrentLevelAsync_HappyPath_ReturnsAssessedLevelDto()
        {
            await SeedSkillLevels();
            _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(EmployeeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeEmployee());
            _repoMock.Setup(r => r.GetPositionSkillsAsync(PositionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PositionSkillRow> { MakePositionSkillRow() });
            _repoMock.Setup(r => r.GetAssessmentAsync(EmployeeId, SkillId, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(default(EmployeeToSkill));
            _repoMock.Setup(r => r.UpsertCurrentLevelAsync(EmployeeId, SkillId, Level2Id, null, EmployeeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EmployeeToSkill { Id = AssessmentId, SkillId = SkillId, SkillLevelId = Level2Id, Notes = null, IsDeleted = false });
            _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await _service.UpsertCurrentLevelAsync(
                EmployeeId, SkillId,
                new UpsertSkillAssessmentDto { SkillLevelId = Level2Id },
                CancellationToken.None);

            Assert.Equal(Level2Id, result.SkillLevelId);
            Assert.Equal("Intermediate", result.SkillLevelTitle);
            Assert.Equal(2, result.SkillLevelValue);
        }

        // ---------- UpsertCurrentLevelAsync — skill_not_found ----------

        [Fact]
        public async Task UpsertCurrentLevelAsync_SkillNotInPosition_ThrowsKeyNotFound()
        {
            _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(EmployeeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeEmployee());
            _repoMock.Setup(r => r.GetPositionSkillsAsync(PositionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PositionSkillRow>()); // empty — skill not in position

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.UpsertCurrentLevelAsync(
                    EmployeeId, SkillId,
                    new UpsertSkillAssessmentDto { SkillLevelId = Level2Id },
                    CancellationToken.None));

            Assert.Contains("skill_not_found", ex.Message);
        }

        // ---------- UpsertCurrentLevelAsync — target_conflict ----------

        [Fact]
        public async Task UpsertCurrentLevelAsync_TargetConflict_ThrowsInvalidOperation()
        {
            await SeedSkillLevels();
            _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(EmployeeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeEmployee());
            _repoMock.Setup(r => r.GetPositionSkillsAsync(PositionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PositionSkillRow> { MakePositionSkillRow() });
            // Target is Level2 (value=2); setting current to Level3 (value=3) → target.value ≤ new current
            _repoMock.Setup(r => r.GetAssessmentAsync(EmployeeId, SkillId, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EmployeeToSkill { Id = Guid.NewGuid(), SkillId = SkillId, SkillLevelId = Level2Id, IsTarget = true, IsDeleted = false });

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.UpsertCurrentLevelAsync(
                    EmployeeId, SkillId,
                    new UpsertSkillAssessmentDto { SkillLevelId = Level3Id },
                    CancellationToken.None));

            Assert.Contains("target_conflict", ex.Message);
        }

        // ---------- UpsertTargetAsync — happy path ----------

        [Fact]
        public async Task UpsertTargetAsync_HappyPath_ReturnsTargetLevelDto()
        {
            await SeedSkillLevels();
            _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(EmployeeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeEmployee());
            _repoMock.Setup(r => r.GetPositionSkillsAsync(PositionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PositionSkillRow> { MakePositionSkillRow() });
            _repoMock.Setup(r => r.GetAssessmentAsync(EmployeeId, SkillId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(default(EmployeeToSkill)); // no current assessment
            _repoMock.Setup(r => r.UpsertTargetLevelAsync(EmployeeId, SkillId, Level4Id, EmployeeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EmployeeToSkill { Id = Guid.NewGuid(), SkillId = SkillId, SkillLevelId = Level4Id, IsTarget = true, IsDeleted = false });
            _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await _service.UpsertTargetAsync(
                EmployeeId, SkillId,
                new UpsertSkillTargetDto { SkillLevelId = Level4Id },
                CancellationToken.None);

            Assert.Equal(Level4Id, result.SkillLevelId);
            Assert.Equal("Expert", result.SkillLevelTitle);
            Assert.Equal(4, result.SkillLevelValue);
        }

        // ---------- UpsertTargetAsync — target_too_low ----------

        [Fact]
        public async Task UpsertTargetAsync_TargetTooLow_ThrowsInvalidOperation()
        {
            await SeedSkillLevels();
            _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(EmployeeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeEmployee());
            _repoMock.Setup(r => r.GetPositionSkillsAsync(PositionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PositionSkillRow> { MakePositionSkillRow() });
            // Current is Level3 (value=3); target Level2 (value=2) → target_too_low
            _repoMock.Setup(r => r.GetAssessmentAsync(EmployeeId, SkillId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EmployeeToSkill { Id = AssessmentId, SkillId = SkillId, SkillLevelId = Level3Id, IsTarget = false, IsDeleted = false });

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.UpsertTargetAsync(
                    EmployeeId, SkillId,
                    new UpsertSkillTargetDto { SkillLevelId = Level2Id },
                    CancellationToken.None));

            Assert.Contains("target_too_low", ex.Message);
        }

        // ---------- LinkEvidenceAsync — happy path ----------

        [Fact]
        public async Task LinkEvidenceAsync_HappyPath_ReturnsEvidenceItemDto()
        {
            _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(EmployeeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeEmployee());
            _repoMock.Setup(r => r.GetPositionSkillsAsync(PositionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PositionSkillRow> { MakePositionSkillRow() });
            _repoMock.Setup(r => r.GetAssessmentAsync(EmployeeId, SkillId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EmployeeToSkill { Id = AssessmentId, SkillId = SkillId, IsTarget = false, IsDeleted = false });
            _repoMock.Setup(r => r.GetFeedbackForEmployeeAsync(FeedbackId, EmployeeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(((bool, string, int?, string)?)(true, "Alice Johnson", (int?)4, "Good feedback content"));
            _repoMock.Setup(r => r.EvidenceLinkExistsAsync(AssessmentId, FeedbackId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _repoMock.Setup(r => r.LinkEvidenceAsync(AssessmentId, FeedbackId, EmployeeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EmployeeSkillEvidence { Id = Guid.NewGuid(), EmployeeToSkillId = AssessmentId, FeedbackId = FeedbackId, IsDeleted = false });
            _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await _service.LinkEvidenceAsync(
                EmployeeId, SkillId,
                new LinkEvidenceDto { FeedbackId = FeedbackId },
                CancellationToken.None);

            Assert.Equal(FeedbackId, result.FeedbackId);
            Assert.Equal("Alice Johnson", result.SenderDisplayName);
            Assert.Equal(4, result.Rating);
        }

        // ---------- LinkEvidenceAsync — assessment_required ----------

        [Fact]
        public async Task LinkEvidenceAsync_NoCurrentAssessment_ThrowsAssessmentRequired()
        {
            _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(EmployeeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeEmployee());
            _repoMock.Setup(r => r.GetPositionSkillsAsync(PositionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PositionSkillRow> { MakePositionSkillRow() });
            _repoMock.Setup(r => r.GetAssessmentAsync(EmployeeId, SkillId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(default(EmployeeToSkill)); // no current assessment

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.LinkEvidenceAsync(
                    EmployeeId, SkillId,
                    new LinkEvidenceDto { FeedbackId = FeedbackId },
                    CancellationToken.None));

            Assert.Contains("assessment_required", ex.Message);
        }

        // ---------- LinkEvidenceAsync — feedback_not_found ----------

        [Fact]
        public async Task LinkEvidenceAsync_FeedbackNotFound_ThrowsKeyNotFound()
        {
            _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(EmployeeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeEmployee());
            _repoMock.Setup(r => r.GetPositionSkillsAsync(PositionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PositionSkillRow> { MakePositionSkillRow() });
            _repoMock.Setup(r => r.GetAssessmentAsync(EmployeeId, SkillId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EmployeeToSkill { Id = AssessmentId, SkillId = SkillId, IsTarget = false, IsDeleted = false });
            _repoMock.Setup(r => r.GetFeedbackForEmployeeAsync(FeedbackId, EmployeeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(default((bool, string, int?, string)?)); // feedback not found

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.LinkEvidenceAsync(
                    EmployeeId, SkillId,
                    new LinkEvidenceDto { FeedbackId = FeedbackId },
                    CancellationToken.None));

            Assert.Contains("feedback_not_found", ex.Message);
        }

        // ---------- LinkEvidenceAsync — already_linked ----------

        [Fact]
        public async Task LinkEvidenceAsync_AlreadyLinked_ThrowsInvalidOperation()
        {
            _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(EmployeeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeEmployee());
            _repoMock.Setup(r => r.GetPositionSkillsAsync(PositionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PositionSkillRow> { MakePositionSkillRow() });
            _repoMock.Setup(r => r.GetAssessmentAsync(EmployeeId, SkillId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EmployeeToSkill { Id = AssessmentId, SkillId = SkillId, IsTarget = false, IsDeleted = false });
            _repoMock.Setup(r => r.GetFeedbackForEmployeeAsync(FeedbackId, EmployeeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(((bool, string, int?, string)?)(true, "Alice Johnson", (int?)4, "Good feedback content"));
            _repoMock.Setup(r => r.EvidenceLinkExistsAsync(AssessmentId, FeedbackId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true); // already linked

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.LinkEvidenceAsync(
                    EmployeeId, SkillId,
                    new LinkEvidenceDto { FeedbackId = FeedbackId },
                    CancellationToken.None));

            Assert.Contains("already_linked", ex.Message);
        }

        // ---------- GetTeamSummaryAsync — correct mapping ----------

        [Fact]
        public async Task GetTeamSummaryAsync_ReturnsMappedTeamMembers()
        {
            var summaries = new List<TeamMemberAssessmentSummary>
            {
                new TeamMemberAssessmentSummary
                {
                    EmployeeId = Guid.NewGuid(),
                    DisplayName = "Jane Smith",
                    PositionTitle = "Senior Engineer",
                    TotalRequiredSkills = 5,
                    AssessedSkillCount = 3,
                    SkillsMeetingRequirementCount = 2
                }
            };
            _repoMock.Setup(r => r.GetTeamSummaryAsync(EmployeeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(summaries);

            var result = await _service.GetTeamSummaryAsync(EmployeeId, CancellationToken.None);

            Assert.Single(result.Team);
            Assert.Equal("Jane Smith", result.Team[0].DisplayName);
            Assert.Equal(5, result.Team[0].TotalRequiredSkills);
            Assert.Equal(3, result.Team[0].AssessedSkillCount);
            Assert.Equal(2, result.Team[0].SkillsMeetingRequirementCount);
        }
    }
}
