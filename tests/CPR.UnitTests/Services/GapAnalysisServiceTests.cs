using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using CPR.Application.Repositories;
using CPR.Domain.Entities;
using CPR.Infrastructure.Services;

namespace CPR.UnitTests.Services
{
    public class GapAnalysisServiceTests
    {
        private readonly Mock<IGapAnalysisRepository> _repoMock;
        private readonly GapAnalysisService _service;

        // ── Shared GUIDs ──────────────────────────────────────────────────────

        private static readonly Guid EmployeeId   = Guid.Parse("aa000000-0000-0000-0000-000000000001");
        private static readonly Guid ManagerId    = Guid.Parse("aa000000-0000-0000-0000-000000000002");
        private static readonly Guid DirectorId   = Guid.Parse("aa000000-0000-0000-0000-000000000003");
        private static readonly Guid AdminId      = Guid.Parse("aa000000-0000-0000-0000-000000000004");
        private static readonly Guid TrackId      = Guid.Parse("bb000000-0000-0000-0000-000000000001");
        private static readonly Guid Position1Id  = Guid.Parse("cc000000-0000-0000-0000-000000000001");
        private static readonly Guid Position2Id  = Guid.Parse("cc000000-0000-0000-0000-000000000002");
        private static readonly Guid SkillId      = Guid.Parse("dd000000-0000-0000-0000-000000000001");
        private static readonly Guid SkillLv1Id   = Guid.Parse("ee000000-0000-0000-0000-000000000001");
        private static readonly Guid SkillLv2Id   = Guid.Parse("ee000000-0000-0000-0000-000000000002");
        private static readonly Guid SkillLv3Id   = Guid.Parse("ee000000-0000-0000-0000-000000000003");
        private static readonly Guid SkillCatId   = Guid.Parse("ff000000-0000-0000-0000-000000000001");
        private static readonly Guid DeptId       = Guid.Parse("99000000-0000-0000-0000-000000000001");

        public GapAnalysisServiceTests()
        {
            _repoMock = new Mock<IGapAnalysisRepository>();
            _service  = new GapAnalysisService(_repoMock.Object);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static CareerTrack MakeTrack() => new CareerTrack
        {
            Id    = TrackId,
            Title = "Engineering",
        };

        private static Position MakePosition(Guid id, int sortOrder) => new Position
        {
            Id           = id,
            Title        = $"Position {sortOrder}",
            CareerTrackId = TrackId,
            SortOrder    = sortOrder,
            CareerTrack  = MakeTrack(),
        };

        private static SkillLevel MakeSkillLevel(Guid id, int value, string title) => new SkillLevel
        {
            Id    = id,
            Value = value,
            Title = title,
        };

        private static Skill MakeSkill() => new Skill
        {
            Id    = SkillId,
            Title = "TypeScript",
            SkillCategory = new SkillCategory { Id = SkillCatId, Title = "Technical" },
            Levels = new List<SkillLevel>
            {
                MakeSkillLevel(SkillLv1Id, 1, "Beginner"),
                MakeSkillLevel(SkillLv2Id, 2, "Mid"),
                MakeSkillLevel(SkillLv3Id, 3, "Senior"),
            },
        };

        private static PositionToSkill MakePts(SkillLevel requiredLevel, bool mandatory = true)
        {
            var skill = MakeSkill();
            return new PositionToSkill
            {
                Id          = Guid.NewGuid(),
                PositionId  = Position2Id,
                SkillId     = SkillId,
                SkillLevelId = requiredLevel.Id,
                IsMandatory = mandatory,
                Skill       = skill,
                SkillLevel  = requiredLevel,
            };
        }

        private static Employee MakeEmployee(
            Guid? managerId = null,
            Guid? positionId = null,
            Guid? deptId = null) => new Employee
        {
            Id           = EmployeeId,
            UserId       = Guid.NewGuid(),
            PositionId   = positionId,
            ManagerId    = managerId,
            DepartmentId = deptId,
            IsDeleted    = false,
        };

        private static Employee MakeCallerEmployee(Guid id, Guid? deptId = null) => new Employee
        {
            Id           = id,
            UserId       = Guid.NewGuid(),
            PositionId   = Position1Id,
            DepartmentId = deptId,
            IsDeleted    = false,
        };

        // ── GetMyGapAnalysisAsync ─────────────────────────────────────────────

        [Fact]
        public async Task GetMyGapAnalysisAsync_EmployeeNotFound_ThrowsKeyNotFound()
        {
            _repoMock.Setup(r => r.GetEmployeeRecordAsync(EmployeeId))
                     .ReturnsAsync((Employee?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.GetMyGapAnalysisAsync(EmployeeId));
        }

        [Fact]
        public async Task GetMyGapAnalysisAsync_NoPositionAssigned_ThrowsInvalidOperation()
        {
            var employee = MakeEmployee(positionId: null);
            _repoMock.Setup(r => r.GetEmployeeRecordAsync(EmployeeId))
                     .ReturnsAsync(employee);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.GetMyGapAnalysisAsync(EmployeeId));
        }

        [Fact]
        public async Task GetMyGapAnalysisAsync_AtHighestLevel_ReturnsNullNextPosition()
        {
            var employee = MakeEmployee(positionId: Position2Id);
            _repoMock.Setup(r => r.GetEmployeeRecordAsync(EmployeeId)).ReturnsAsync(employee);
            _repoMock.Setup(r => r.GetPositionByIdAsync(Position2Id))
                     .ReturnsAsync(MakePosition(Position2Id, 2));
            _repoMock.Setup(r => r.GetNextPositionAsync(Position2Id, TrackId, 2))
                     .ReturnsAsync((Position?)null);

            var result = await _service.GetMyGapAnalysisAsync(EmployeeId);

            Assert.Null(result.NextPosition);
            Assert.Empty(result.SkillGaps);
        }

        [Fact]
        public async Task GetMyGapAnalysisAsync_ManagerAssessmentPresent_UsesManagerValue()
        {
            var requiredLevel = MakeSkillLevel(SkillLv3Id, 3, "Senior");
            var actualLevel   = MakeSkillLevel(SkillLv2Id, 2, "Mid");
            var pts           = MakePts(requiredLevel);

            var employee = MakeEmployee(positionId: Position1Id);
            var empSkill = new EmployeeToSkill
            {
                EmployeeId = EmployeeId,
                SkillId    = SkillId,
                ManagerAssessmentValue = 2,
            };

            _repoMock.Setup(r => r.GetEmployeeRecordAsync(EmployeeId)).ReturnsAsync(employee);
            _repoMock.Setup(r => r.GetPositionByIdAsync(Position1Id))
                     .ReturnsAsync(MakePosition(Position1Id, 1));
            _repoMock.Setup(r => r.GetNextPositionAsync(Position1Id, TrackId, 1))
                     .ReturnsAsync(MakePosition(Position2Id, 2));
            _repoMock.Setup(r => r.GetPositionSkillsAsync(Position2Id))
                     .ReturnsAsync(new List<PositionToSkill> { pts });
            _repoMock.Setup(r => r.GetEmployeeSkillsAsync(EmployeeId))
                     .ReturnsAsync(new List<EmployeeToSkill> { empSkill });
            _repoMock.Setup(r => r.GetLinkedGoalsAsync(EmployeeId, It.IsAny<IEnumerable<Guid>>()))
                     .ReturnsAsync(new List<Goal>());

            var result = await _service.GetMyGapAnalysisAsync(EmployeeId);

            Assert.Single(result.SkillGaps);
            var gap = result.SkillGaps[0];
            Assert.Equal("manager", gap.AssessmentSource);
            Assert.Equal(2, gap.ActualLevel.Value);
            Assert.Equal(1, gap.Gap); // 3 - 2 = 1
        }

        [Fact]
        public async Task GetMyGapAnalysisAsync_NoManagerAssessment_UsesDefaultMinimumLevel()
        {
            var requiredLevel = MakeSkillLevel(SkillLv3Id, 3, "Senior");
            var minLevel      = MakeSkillLevel(SkillLv1Id, 1, "Beginner");
            var pts           = MakePts(requiredLevel);

            var employee = MakeEmployee(positionId: Position1Id);

            _repoMock.Setup(r => r.GetEmployeeRecordAsync(EmployeeId)).ReturnsAsync(employee);
            _repoMock.Setup(r => r.GetPositionByIdAsync(Position1Id))
                     .ReturnsAsync(MakePosition(Position1Id, 1));
            _repoMock.Setup(r => r.GetNextPositionAsync(Position1Id, TrackId, 1))
                     .ReturnsAsync(MakePosition(Position2Id, 2));
            _repoMock.Setup(r => r.GetPositionSkillsAsync(Position2Id))
                     .ReturnsAsync(new List<PositionToSkill> { pts });
            _repoMock.Setup(r => r.GetEmployeeSkillsAsync(EmployeeId))
                     .ReturnsAsync(new List<EmployeeToSkill>()); // no assessment
            _repoMock.Setup(r => r.GetMinimumSkillLevelAsync(SkillId))
                     .ReturnsAsync(minLevel);
            _repoMock.Setup(r => r.GetLinkedGoalsAsync(EmployeeId, It.IsAny<IEnumerable<Guid>>()))
                     .ReturnsAsync(new List<Goal>());

            var result = await _service.GetMyGapAnalysisAsync(EmployeeId);

            var gap = result.SkillGaps[0];
            Assert.Equal("default", gap.AssessmentSource);
            Assert.Equal(1, gap.ActualLevel.Value);
            Assert.Equal(2, gap.Gap); // 3 - 1 = 2
        }

        [Fact]
        public async Task GetMyGapAnalysisAsync_GapMet_LinkedGoalsNotIncluded()
        {
            // Required = 2, Actual = 2 → gap = 0 → no linked goals shown.
            var requiredLevel = MakeSkillLevel(SkillLv2Id, 2, "Mid");
            var pts           = MakePts(requiredLevel);
            var employee      = MakeEmployee(positionId: Position1Id);
            var empSkill      = new EmployeeToSkill
            {
                EmployeeId = EmployeeId,
                SkillId    = SkillId,
                ManagerAssessmentValue = 2,
            };
            var linkedGoal = new Goal
            {
                Id             = Guid.NewGuid(),
                Title          = "Some goal",
                RelatedSkillId = SkillId,
                ProgressPercent = 10,
            };

            _repoMock.Setup(r => r.GetEmployeeRecordAsync(EmployeeId)).ReturnsAsync(employee);
            _repoMock.Setup(r => r.GetPositionByIdAsync(Position1Id))
                     .ReturnsAsync(MakePosition(Position1Id, 1));
            _repoMock.Setup(r => r.GetNextPositionAsync(Position1Id, TrackId, 1))
                     .ReturnsAsync(MakePosition(Position2Id, 2));
            _repoMock.Setup(r => r.GetPositionSkillsAsync(Position2Id))
                     .ReturnsAsync(new List<PositionToSkill> { pts });
            _repoMock.Setup(r => r.GetEmployeeSkillsAsync(EmployeeId))
                     .ReturnsAsync(new List<EmployeeToSkill> { empSkill });
            _repoMock.Setup(r => r.GetLinkedGoalsAsync(EmployeeId, It.IsAny<IEnumerable<Guid>>()))
                     .ReturnsAsync(new List<Goal> { linkedGoal });

            var result = await _service.GetMyGapAnalysisAsync(EmployeeId);

            var gap = result.SkillGaps[0];
            Assert.Equal(0, gap.Gap);
            Assert.Empty(gap.LinkedGoals); // gap = 0, so no linked goals
        }

        [Fact]
        public async Task GetMyGapAnalysisAsync_GapPositive_LinkedGoalsIncluded()
        {
            var requiredLevel = MakeSkillLevel(SkillLv3Id, 3, "Senior");
            var pts           = MakePts(requiredLevel);
            var employee      = MakeEmployee(positionId: Position1Id);
            var empSkill      = new EmployeeToSkill
            {
                EmployeeId = EmployeeId,
                SkillId    = SkillId,
                ManagerAssessmentValue = 1,
            };
            var linkedGoal = new Goal
            {
                Id              = Guid.NewGuid(),
                Title           = "Improve TypeScript",
                RelatedSkillId  = SkillId,
                ProgressPercent = 45,
                Status          = "in_progress",
            };

            _repoMock.Setup(r => r.GetEmployeeRecordAsync(EmployeeId)).ReturnsAsync(employee);
            _repoMock.Setup(r => r.GetPositionByIdAsync(Position1Id))
                     .ReturnsAsync(MakePosition(Position1Id, 1));
            _repoMock.Setup(r => r.GetNextPositionAsync(Position1Id, TrackId, 1))
                     .ReturnsAsync(MakePosition(Position2Id, 2));
            _repoMock.Setup(r => r.GetPositionSkillsAsync(Position2Id))
                     .ReturnsAsync(new List<PositionToSkill> { pts });
            _repoMock.Setup(r => r.GetEmployeeSkillsAsync(EmployeeId))
                     .ReturnsAsync(new List<EmployeeToSkill> { empSkill });
            _repoMock.Setup(r => r.GetLinkedGoalsAsync(EmployeeId, It.IsAny<IEnumerable<Guid>>()))
                     .ReturnsAsync(new List<Goal> { linkedGoal });

            var result = await _service.GetMyGapAnalysisAsync(EmployeeId);

            var gap = result.SkillGaps[0];
            Assert.Equal(2, gap.Gap);
            Assert.Single(gap.LinkedGoals);
            Assert.Equal("Improve TypeScript", gap.LinkedGoals[0].Title);
            Assert.Equal(45, gap.LinkedGoals[0].ProgressPercentage);
        }

        [Fact]
        public async Task GetMyGapAnalysisAsync_SummaryCountsCorrect()
        {
            // Two skills: one met (gap=0), one with gap (mandatory).
            var lv2 = MakeSkillLevel(SkillLv2Id, 2, "Mid");
            var lv3 = MakeSkillLevel(SkillLv3Id, 3, "Senior");

            var skill2Id   = Guid.Parse("dd000000-0000-0000-0000-000000000002");
            var skill2CatId = Guid.Parse("ff000000-0000-0000-0000-000000000002");

            var pts1 = MakePts(lv2, mandatory: false); // met
            var skill2 = new Skill
            {
                Id            = skill2Id,
                Title         = "React",
                SkillCategory = new SkillCategory { Id = skill2CatId, Title = "Frontend" },
                Levels        = new List<SkillLevel> { lv2, lv3 },
            };
            var pts2 = new PositionToSkill
            {
                Id           = Guid.NewGuid(),
                PositionId   = Position2Id,
                SkillId      = skill2Id,
                SkillLevelId = lv3.Id,
                IsMandatory  = true,
                Skill        = skill2,
                SkillLevel   = lv3,
            };

            var employee = MakeEmployee(positionId: Position1Id);
            var empSkill1 = new EmployeeToSkill { EmployeeId = EmployeeId, SkillId = SkillId,   ManagerAssessmentValue = 2 };
            var empSkill2 = new EmployeeToSkill { EmployeeId = EmployeeId, SkillId = skill2Id,  ManagerAssessmentValue = 1 };

            _repoMock.Setup(r => r.GetEmployeeRecordAsync(EmployeeId)).ReturnsAsync(employee);
            _repoMock.Setup(r => r.GetPositionByIdAsync(Position1Id))
                     .ReturnsAsync(MakePosition(Position1Id, 1));
            _repoMock.Setup(r => r.GetNextPositionAsync(Position1Id, TrackId, 1))
                     .ReturnsAsync(MakePosition(Position2Id, 2));
            _repoMock.Setup(r => r.GetPositionSkillsAsync(Position2Id))
                     .ReturnsAsync(new List<PositionToSkill> { pts1, pts2 });
            _repoMock.Setup(r => r.GetEmployeeSkillsAsync(EmployeeId))
                     .ReturnsAsync(new List<EmployeeToSkill> { empSkill1, empSkill2 });
            _repoMock.Setup(r => r.GetLinkedGoalsAsync(EmployeeId, It.IsAny<IEnumerable<Guid>>()))
                     .ReturnsAsync(new List<Goal>());

            var result = await _service.GetMyGapAnalysisAsync(EmployeeId);

            Assert.Equal(2, result.Summary.TotalSkills);
            Assert.Equal(1, result.Summary.SkillsMet);
            Assert.Equal(1, result.Summary.SkillsWithGap);
            Assert.Equal(1, result.Summary.MandatoryGaps);
        }

        // ── GetEmployeeGapAnalysisAsync — Authorization ───────────────────────

        [Fact]
        public async Task GetEmployeeGapAnalysisAsync_Administrator_Unrestricted()
        {
            var target    = MakeEmployee(positionId: Position1Id);
            var requiredLevel = MakeSkillLevel(SkillLv2Id, 2, "Mid");
            var pts       = MakePts(requiredLevel);

            _repoMock.Setup(r => r.GetEmployeeRecordAsync(EmployeeId)).ReturnsAsync(target);
            _repoMock.Setup(r => r.GetPositionByIdAsync(Position1Id))
                     .ReturnsAsync(MakePosition(Position1Id, 1));
            _repoMock.Setup(r => r.GetNextPositionAsync(Position1Id, TrackId, 1))
                     .ReturnsAsync((Position?)null); // at highest level
            // No DepartmentId check needed for Administrator.

            var result = await _service.GetEmployeeGapAnalysisAsync(EmployeeId, AdminId, "Administrator");

            Assert.Null(result.NextPosition);
        }

        [Fact]
        public async Task GetEmployeeGapAnalysisAsync_Director_SameDepartment_Allowed()
        {
            var target   = MakeEmployee(positionId: Position1Id, deptId: DeptId);
            var director = MakeCallerEmployee(DirectorId, deptId: DeptId);

            _repoMock.Setup(r => r.GetEmployeeRecordAsync(EmployeeId)).ReturnsAsync(target);
            _repoMock.Setup(r => r.GetEmployeeRecordAsync(DirectorId)).ReturnsAsync(director);
            _repoMock.Setup(r => r.GetPositionByIdAsync(Position1Id))
                     .ReturnsAsync(MakePosition(Position1Id, 1));
            _repoMock.Setup(r => r.GetNextPositionAsync(Position1Id, TrackId, 1))
                     .ReturnsAsync((Position?)null);

            var result = await _service.GetEmployeeGapAnalysisAsync(EmployeeId, DirectorId, "Director");
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetEmployeeGapAnalysisAsync_Director_DifferentDepartment_ThrowsUnauthorized()
        {
            var otherDeptId = Guid.Parse("99000000-0000-0000-0000-000000000099");
            var target      = MakeEmployee(positionId: Position1Id, deptId: DeptId);
            var director    = MakeCallerEmployee(DirectorId, deptId: otherDeptId);

            _repoMock.Setup(r => r.GetEmployeeRecordAsync(EmployeeId)).ReturnsAsync(target);
            _repoMock.Setup(r => r.GetEmployeeRecordAsync(DirectorId)).ReturnsAsync(director);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _service.GetEmployeeGapAnalysisAsync(EmployeeId, DirectorId, "Director"));
        }

        [Fact]
        public async Task GetEmployeeGapAnalysisAsync_PeopleManager_DirectReport_Allowed()
        {
            var target = MakeEmployee(positionId: Position1Id, managerId: ManagerId);

            _repoMock.Setup(r => r.GetEmployeeRecordAsync(EmployeeId)).ReturnsAsync(target);
            _repoMock.Setup(r => r.GetPositionByIdAsync(Position1Id))
                     .ReturnsAsync(MakePosition(Position1Id, 1));
            _repoMock.Setup(r => r.GetNextPositionAsync(Position1Id, TrackId, 1))
                     .ReturnsAsync((Position?)null);

            var result = await _service.GetEmployeeGapAnalysisAsync(EmployeeId, ManagerId, "People Manager");
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetEmployeeGapAnalysisAsync_PeopleManager_NotDirectReport_ThrowsUnauthorized()
        {
            var anotherManagerId = Guid.Parse("aa000000-0000-0000-0000-000000000099");
            var target = MakeEmployee(positionId: Position1Id, managerId: anotherManagerId);

            _repoMock.Setup(r => r.GetEmployeeRecordAsync(EmployeeId)).ReturnsAsync(target);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _service.GetEmployeeGapAnalysisAsync(EmployeeId, ManagerId, "People Manager"));
        }

        [Fact]
        public async Task GetEmployeeGapAnalysisAsync_UnrecognizedRole_ThrowsUnauthorized()
        {
            var target = MakeEmployee(positionId: Position1Id);
            _repoMock.Setup(r => r.GetEmployeeRecordAsync(EmployeeId)).ReturnsAsync(target);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _service.GetEmployeeGapAnalysisAsync(EmployeeId, AdminId, "Employee"));
        }

        [Fact]
        public async Task GetEmployeeGapAnalysisAsync_TargetNotFound_ThrowsKeyNotFound()
        {
            _repoMock.Setup(r => r.GetEmployeeRecordAsync(EmployeeId))
                     .ReturnsAsync((Employee?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.GetEmployeeGapAnalysisAsync(EmployeeId, AdminId, "Administrator"));
        }
    }
}
