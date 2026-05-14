using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using CPR.Application.DTOs.SkillAssessment;
using CPR.Application.DTOs.Taxonomy;
using CPR.Application.Repositories;
using CPR.Domain.Entities;
using CPR.Domain.Repositories;
using CPR.Infrastructure.Data;
using CPR.Infrastructure.Services;

namespace CPR.UnitTests.Services
{
    public class SkillAssessmentServiceTests : IDisposable
    {
        private readonly CprDbContext _db;
        private readonly Mock<ISkillAssessmentRepository> _repoMock;
        private readonly Mock<IAnalyticsRepository> _analyticsRepoMock;
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
            _analyticsRepoMock = new Mock<IAnalyticsRepository>();

            // Default stub: AddSkillHistorySnapshotAsync and SaveChangesAsync succeed silently
            _analyticsRepoMock
                .Setup(r => r.AddSkillHistorySnapshotAsync(It.IsAny<EmployeeSkillHistory>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _analyticsRepoMock
                .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _service = new SkillAssessmentService(_repoMock.Object, _db, _analyticsRepoMock.Object);
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
        public async Task UpsertCurrentLevelAsync_HappyPath_ReturnsSkillAssessmentResponse()
        {
            _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(EmployeeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeEmployee());
            _repoMock.Setup(r => r.GetPositionSkillsAsync(PositionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PositionSkillRow> { MakePositionSkillRow() });
            _repoMock.Setup(r => r.UpsertCurrentLevelAsync(EmployeeId, SkillId, 3.0m, null, EmployeeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EmployeeToSkill { Id = AssessmentId, SkillId = SkillId, SelfAssessmentValue = 3.0m, Notes = null, IsDeleted = false });
            _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.GetEmployeeAssessmentsAsync(EmployeeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<EmployeeSkillRow>());

            var result = await _service.UpsertCurrentLevelAsync(
                EmployeeId, SkillId,
                new UpsertSkillAssessmentDto { SelfAssessmentValue = 3.0m },
                CancellationToken.None);

            // UpsertCurrentLevelAsync returns SkillAssessmentResponseDto (full response, no target_level/weight).
            Assert.NotNull(result);
            // Verify the repo upsert was called with the correct arguments.
            _repoMock.Verify(r => r.UpsertCurrentLevelAsync(EmployeeId, SkillId, 3.0m, null, EmployeeId, It.IsAny<CancellationToken>()), Times.Once);
        }

        // ---------- AC-027/AC-028: History snapshot on create/self_assessment update ----------

        [Fact(DisplayName = "AC-027 AC-028: UpsertCurrentLevelAsync calls AddSkillHistorySnapshotAsync with the new assessment values")]
        public async Task UpsertCurrentLevelAsync_CallsAddSkillHistorySnapshot()
        {
            _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(EmployeeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeEmployee());
            _repoMock.Setup(r => r.GetPositionSkillsAsync(PositionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PositionSkillRow> { MakePositionSkillRow() });
            _repoMock.Setup(r => r.UpsertCurrentLevelAsync(EmployeeId, SkillId, 3.0m, null, EmployeeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EmployeeToSkill { Id = AssessmentId, SkillId = SkillId, SelfAssessmentValue = 3.0m, Notes = null, IsDeleted = false });
            _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.GetEmployeeAssessmentsAsync(EmployeeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<EmployeeSkillRow>());

            await _service.UpsertCurrentLevelAsync(
                EmployeeId, SkillId,
                new UpsertSkillAssessmentDto { SelfAssessmentValue = 3.0m },
                CancellationToken.None);

            // AC-027/AC-028: a history snapshot must be written
            _analyticsRepoMock.Verify(
                r => r.AddSkillHistorySnapshotAsync(
                    It.Is<EmployeeSkillHistory>(h =>
                        h.EmployeeId == EmployeeId &&
                        h.SkillId == SkillId &&
                        h.SelfAssessmentValue == 3.0m),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // ---------- AC-029: History snapshot on manager_assessment update ----------

        [Fact(DisplayName = "AC-029: UpsertManagerAssessmentAsync calls AddSkillHistorySnapshotAsync with manager value")]
        public async Task UpsertManagerAssessmentAsync_CallsAddSkillHistorySnapshot()
        {
            _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(ManagerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeManager());
            _repoMock.Setup(r => r.GetEmployeeWithPositionAsync(EmployeeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeEmployee(managerId: ManagerId));
            _repoMock.Setup(r => r.UpsertManagerAssessmentAsync(EmployeeId, SkillId, 4.0m, ManagerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EmployeeToSkill { Id = AssessmentId, SkillId = SkillId, SelfAssessmentValue = 2.0m, ManagerAssessmentValue = 4.0m, IsDeleted = false });
            _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.GetPositionSkillsAsync(PositionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PositionSkillRow>());
            _repoMock.Setup(r => r.GetEmployeeAssessmentsAsync(EmployeeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<EmployeeSkillRow>());

            await _service.UpsertManagerAssessmentAsync(
                ManagerId, "People Manager", EmployeeId, SkillId, 4.0m, CancellationToken.None);

            // AC-029: history snapshot must include the new manager assessment value
            _analyticsRepoMock.Verify(
                r => r.AddSkillHistorySnapshotAsync(
                    It.Is<EmployeeSkillHistory>(h =>
                        h.EmployeeId == EmployeeId &&
                        h.SkillId == SkillId &&
                        h.ManagerAssessmentValue == 4.0m),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // ---------- AC-030: History rows immutable — no delete/update path ----------

        [Fact(DisplayName = "AC-030: employee_skill_history rows are never modified or deleted — IAnalyticsRepository exposes no delete method")]
        public void AnalyticsRepository_DoesNotExposeDeleteHistoryMethod()
        {
            // AC-030: The IAnalyticsRepository interface must NOT have any method that modifies
            // or removes existing EmployeeSkillHistory rows.
            var repoType = typeof(IAnalyticsRepository);
            var methods = repoType.GetMethods();

            // Verify no delete/remove/update history method exists
            var deleteMethods = methods.Where(m =>
                m.Name.Contains("Delete") || m.Name.Contains("Remove") || m.Name.Contains("Update")
            ).Where(m => m.Name.Contains("History") || m.Name.Contains("Snapshot"));

            Assert.Empty(deleteMethods);
        }

        // ---------- AC-012: DeleteCurrentLevelAsync — removes assessment ----------

        [Fact]
        public async Task DeleteCurrentLevelAsync_CallsRepoDelete()
        {
            // AC-012: DELETE removes the self-assessment row
            _repoMock.Setup(r => r.DeleteCurrentLevelAsync(EmployeeId, SkillId, EmployeeId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            await _service.DeleteCurrentLevelAsync(EmployeeId, SkillId, CancellationToken.None);

            _repoMock.Verify(r => r.DeleteCurrentLevelAsync(EmployeeId, SkillId, EmployeeId, It.IsAny<CancellationToken>()), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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
                ManagerId, "People Manager", EmployeeId, SkillId, 4.0m, CancellationToken.None);

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
                    ManagerId, "People Manager", EmployeeId, SkillId, 4.0m, CancellationToken.None));

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
                    ManagerId, "People Manager", EmployeeId, SkillId, 4.0m, CancellationToken.None));

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

        // ---------- AC-002/AC-003: Response DTO shape — no target_level, no weight ----------

        [Fact]
        public void SkillItemDto_DoesNotExpose_TargetLevel()
        {
            // AC-002: no target_level field in the skill item DTO
            var type = typeof(CPR.Application.DTOs.SkillAssessment.SkillItemDto);
            Assert.Null(type.GetProperty("TargetLevel"));
            Assert.Null(type.GetProperty("Target"));
        }

        [Fact]
        public void SkillAssessmentResponseDto_DoesNotExpose_Weight()
        {
            // AC-003: no weight field anywhere in the assessment response
            var skillItemType = typeof(CPR.Application.DTOs.SkillAssessment.SkillItemDto);
            Assert.Null(skillItemType.GetProperty("Weight"));
            var positionSkillType = typeof(CPR.Application.DTOs.Taxonomy.PositionSkillRequirementDto);
            Assert.Null(positionSkillType.GetProperty("Weight"));
        }

        // ---------- AC-018: employee_skill_evidence entity exists ----------

        [Fact]
        public void EmployeeSkillEvidence_EntityExists_WithCorrectForeignKeys()
        {
            // AC-018: evidence links stored in employee_skill_evidence table
            var entity = new EmployeeSkillEvidence
            {
                Id = Guid.NewGuid(),
                EmployeeToSkillId = AssessmentId,
                FeedbackId = FeedbackId,
                IsDeleted = false
            };
            Assert.Equal(AssessmentId, entity.EmployeeToSkillId);
            Assert.Equal(FeedbackId, entity.FeedbackId);
        }

        // ---------- AC-022: TargetLevel types removed from codebase ----------

        [Fact]
        public void TargetLevelTypes_AreNotPresent()
        {
            // AC-022: UpsertSkillTargetDto and TargetLevelDto must not exist
            var assembly = typeof(CPR.Application.DTOs.SkillAssessment.UpsertSkillAssessmentDto).Assembly;
            var types = assembly.GetTypes();
            Assert.DoesNotContain(types, t => t.Name == "UpsertSkillTargetDto");
            Assert.DoesNotContain(types, t => t.Name == "TargetLevelDto");
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
