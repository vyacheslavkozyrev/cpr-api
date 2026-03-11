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

        private static readonly Guid EmployeeId     = Guid.Parse("aa000000-0000-0000-0000-000000000001");
        private static readonly Guid ManagerId      = Guid.Parse("aa000000-0000-0000-0000-000000000002");
        private static readonly Guid DirectorUserId = Guid.Parse("aa000000-0000-0000-0000-000000000003");
        private static readonly Guid DirectorId     = Guid.Parse("aa000000-0000-0000-0000-000000000004");
        private static readonly Guid PositionId     = Guid.Parse("bb000000-0000-0000-0000-000000000001");
        private static readonly Guid SkillId        = Guid.Parse("cc000000-0000-0000-0000-000000000001");
        private static readonly Guid FeedbackId     = Guid.Parse("ee000000-0000-0000-0000-000000000001");
        private static readonly Guid AssessmentId   = Guid.Parse("ff000000-0000-0000-0000-000000000001");

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

        private static Employee MakeEmployee(Guid? managerId = null) => new Employee
        {
            Id = EmployeeId,
            UserId = Guid.NewGuid(),
            PositionId = PositionId,
            ManagerId = managerId,
            IsDeleted = false
        };

        private static Employee MakeManager() => new Employee
        {
            Id = ManagerId,
            UserId = Guid.Parse("aa000000-0000-0000-0000-000000000099"),
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
            RequiredLevelId = Guid.NewGuid(),
            RequiredLevelTitle = "Advanced",
            RequiredLevelValue = 3
        };

        // ---------- UpsertCurrentLevelAsync — happy path ----------

        [Fact]
        public async Task UpsertCurrentLevelAsync_HappyPath_ReturnsAssessedLevelDto()
        {
            _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(EmployeeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeEmployee());
            _repoMock.Setup(r => r.GetPositionSkillsAsync(PositionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PositionSkillRow> { MakePositionSkillRow() });
            _repoMock.Setup(r => r.UpsertCurrentLevelAsync(EmployeeId, SkillId, 3.0m, null, EmployeeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EmployeeToSkill { Id = AssessmentId, SkillId = SkillId, SelfAssessmentValue = 3.0m, Notes = null, IsDeleted = false });
            _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await _service.UpsertCurrentLevelAsync(
                EmployeeId, SkillId,
                new UpsertSkillAssessmentDto { SelfAssessmentValue = 3.0m },
                CancellationToken.None);

            Assert.Equal(3.0m, result.SelfAssessmentValue);
            Assert.Null(result.ManagerAssessmentValue);
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
                    new UpsertSkillAssessmentDto { SelfAssessmentValue = 2.5m },
                    CancellationToken.None));

            Assert.Contains("skill_not_found", ex.Message);
        }

        // ---------- UpsertManagerAssessmentAsync — happy path (direct report) ----------

        [Fact]
        public async Task UpsertManagerAssessmentAsync_DirectReport_ReturnsAssessment()
        {
            // Manager is the actor; employee.ManagerId = ManagerId
            _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(ManagerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeManager());
            _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(EmployeeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeEmployee(managerId: ManagerId));
            _repoMock.Setup(r => r.UpsertManagerAssessmentAsync(EmployeeId, SkillId, 4.0m, ManagerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EmployeeToSkill { Id = AssessmentId, SkillId = SkillId, SelfAssessmentValue = 2.0m, ManagerAssessmentValue = 4.0m, IsDeleted = false });
            _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            // BuildAssessmentResponseAsync needs employee with position
            _repoMock.Setup(r => r.GetPositionSkillsAsync(PositionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PositionSkillRow>());
            _repoMock.Setup(r => r.GetEmployeeAssessmentsAsync(EmployeeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<EmployeeSkillRow>());

            var result = await _service.UpsertManagerAssessmentAsync(
                ManagerId, EmployeeId, SkillId, 4.0m, CancellationToken.None);

            Assert.NotNull(result);
        }

        // ---------- UpsertManagerAssessmentAsync — 403 when PeopleManager targets non-direct-report ----------

        [Fact]
        public async Task UpsertManagerAssessmentAsync_NonDirectReport_ThrowsUnauthorized()
        {
            // Employee's manager is someone else, not ManagerId
            var otherManagerId = Guid.Parse("aa000000-0000-0000-0000-000000000099");
            _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(ManagerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeManager());
            _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(EmployeeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeEmployee(managerId: otherManagerId)); // different manager

            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _service.UpsertManagerAssessmentAsync(
                    ManagerId, EmployeeId, SkillId, 4.0m, CancellationToken.None));

            Assert.Contains("forbidden", ex.Message);
        }

        // ---------- UpsertManagerAssessmentAsync — 404 when skill row not found ----------

        [Fact]
        public async Task UpsertManagerAssessmentAsync_AssessmentNotFound_ThrowsKeyNotFound()
        {
            _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(ManagerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeManager());
            _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(EmployeeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeEmployee(managerId: ManagerId));
            _repoMock.Setup(r => r.UpsertManagerAssessmentAsync(EmployeeId, SkillId, 4.0m, ManagerId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException("assessment_not_found"));

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.UpsertManagerAssessmentAsync(
                    ManagerId, EmployeeId, SkillId, 4.0m, CancellationToken.None));

            Assert.Contains("assessment_not_found", ex.Message);
        }

        // ---------- LinkEvidenceAsync — happy path ----------

        [Fact]
        public async Task LinkEvidenceAsync_HappyPath_ReturnsEvidenceItemDto()
        {
            _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(EmployeeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeEmployee());
            _repoMock.Setup(r => r.GetPositionSkillsAsync(PositionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PositionSkillRow> { MakePositionSkillRow() });
            _repoMock.Setup(r => r.GetAssessmentAsync(EmployeeId, SkillId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EmployeeToSkill { Id = AssessmentId, SkillId = SkillId, IsDeleted = false });
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
            _repoMock.Setup(r => r.GetAssessmentAsync(EmployeeId, SkillId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(default(EmployeeToSkill));

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
            _repoMock.Setup(r => r.GetAssessmentAsync(EmployeeId, SkillId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EmployeeToSkill { Id = AssessmentId, SkillId = SkillId, IsDeleted = false });
            _repoMock.Setup(r => r.GetFeedbackForEmployeeAsync(FeedbackId, EmployeeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(default((bool, string, int?, string)?));

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
            _repoMock.Setup(r => r.GetAssessmentAsync(EmployeeId, SkillId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EmployeeToSkill { Id = AssessmentId, SkillId = SkillId, IsDeleted = false });
            _repoMock.Setup(r => r.GetFeedbackForEmployeeAsync(FeedbackId, EmployeeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(((bool, string, int?, string)?)(true, "Alice Johnson", (int?)4, "Good feedback content"));
            _repoMock.Setup(r => r.EvidenceLinkExistsAsync(AssessmentId, FeedbackId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

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
