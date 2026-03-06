#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using CPR.Application.DTOs.Taxonomy;
using CPR.Domain.Entities;
using CPR.Domain.Repositories;
using CPR.Infrastructure.Data;
using CPR.Infrastructure.Services;

namespace CPR.UnitTests.Services
{
    public class TaxonomyServiceTests : IDisposable
    {
        private readonly CprDbContext _db;
        private readonly Mock<ITaxonomyRepository> _repoMock;
        private readonly TaxonomyService _service;

        private static readonly Guid AdminId = Guid.Parse("aa000000-0000-0000-0000-000000000001");

        public TaxonomyServiceTests()
        {
            var options = new DbContextOptionsBuilder<CprDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new CprDbContext(options);
            _repoMock = new Mock<ITaxonomyRepository>();
            _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
            _service = new TaxonomyService(_db, _repoMock.Object);
        }

        public void Dispose() => _db.Dispose();

        // ==================== Helpers ====================

        private CareerPath MakeCareerPath(string title = "Engineering", bool deleted = false)
        {
            var cp = new CareerPath { Id = Guid.NewGuid(), Title = title, IsDeleted = deleted, CreatedAt = DateTimeOffset.UtcNow };
            _db.CareerPaths.Add(cp);
            _db.SaveChanges();
            return cp;
        }

        private CareerTrack MakeCareerTrack(Guid careerPathId, string title = "Backend", bool deleted = false)
        {
            var ct = new CareerTrack { Id = Guid.NewGuid(), Title = title, CareerPathId = careerPathId, IsDeleted = deleted, CreatedAt = DateTimeOffset.UtcNow };
            _db.CareerTracks.Add(ct);
            _db.SaveChanges();
            return ct;
        }

        private Position MakePosition(Guid trackId, string title = "Senior Dev", int sortOrder = 0, bool deleted = false)
        {
            var p = new Position { Id = Guid.NewGuid(), Title = title, CareerTrackId = trackId, SortOrder = sortOrder, IsDeleted = deleted, CreatedAt = DateTimeOffset.UtcNow };
            _db.Positions.Add(p);
            _db.SaveChanges();
            return p;
        }

        private SkillCategory MakeSkillCategory(string title = "Technical", bool deleted = false)
        {
            var sc = new SkillCategory { Id = Guid.NewGuid(), Title = title, IsDeleted = deleted, CreatedAt = DateTimeOffset.UtcNow };
            _db.SkillCategories.Add(sc);
            _db.SaveChanges();
            return sc;
        }

        private Skill MakeSkill(Guid categoryId, string title = "C#", bool deleted = false)
        {
            var s = new Skill { Id = Guid.NewGuid(), Title = title, CategoryId = categoryId, IsDeleted = deleted, CreatedAt = DateTimeOffset.UtcNow };
            _db.Skills.Add(s);
            _db.SaveChanges();
            return s;
        }

        private SkillLevel MakeSkillLevel(Guid skillId, int value = 3, string title = "Intermediate", bool deleted = false)
        {
            var sl = new SkillLevel { Id = Guid.NewGuid(), Title = title, Value = value, SkillId = skillId, IsDeleted = deleted, CreatedAt = DateTimeOffset.UtcNow };
            _db.SkillLevels.Add(sl);
            _db.SaveChanges();
            return sl;
        }

        private PositionToSkill MakePositionSkill(Guid positionId, Guid skillId, Guid skillLevelId, bool deleted = false)
        {
            var pts = new PositionToSkill { Id = Guid.NewGuid(), PositionId = positionId, SkillId = skillId, SkillLevelId = skillLevelId, IsMandatory = true, IsDeleted = deleted, CreatedAt = DateTimeOffset.UtcNow };
            _db.PositionToSkills.Add(pts);
            _db.SaveChanges();
            return pts;
        }

        // ==================== GetCareerPathsAsync ====================

        [Fact]
        public async Task GetCareerPathsAsync_EmptyDb_ReturnsZeroItems()
        {
            var result = await _service.GetCareerPathsAsync(1, 20, "title", "asc");
            Assert.Equal(0, result.TotalItems);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetCareerPathsAsync_SoftDeletedFiltered_ReturnsOnlyActive()
        {
            MakeCareerPath("Active Path");
            MakeCareerPath("Deleted Path", deleted: true);

            var result = await _service.GetCareerPathsAsync(1, 20, "title", "asc");
            Assert.Equal(1, result.TotalItems);
            Assert.Equal("Active Path", result.Data[0].Title);
        }

        [Fact]
        public async Task GetCareerPathsAsync_InvalidSortField_Throws()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.GetCareerPathsAsync(1, 20, "invalid_field", "asc"));
        }

        [Fact]
        public async Task GetCareerPathsAsync_InvalidSortDir_Throws()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.GetCareerPathsAsync(1, 20, "title", "sideways"));
        }

        // ==================== GetCareerPathByIdAsync ====================

        [Fact]
        public async Task GetCareerPathByIdAsync_NotFound_ReturnsNull()
        {
            var result = await _service.GetCareerPathByIdAsync(Guid.NewGuid());
            Assert.Null(result);
        }

        [Fact]
        public async Task GetCareerPathByIdAsync_DeletedPath_ReturnsNull()
        {
            var cp = MakeCareerPath(deleted: true);
            var result = await _service.GetCareerPathByIdAsync(cp.Id);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetCareerPathByIdAsync_IncludesOnlyActiveTracks()
        {
            var cp = MakeCareerPath();
            MakeCareerTrack(cp.Id, "Active Track");
            MakeCareerTrack(cp.Id, "Deleted Track", deleted: true);

            var result = await _service.GetCareerPathByIdAsync(cp.Id);
            Assert.NotNull(result);
            Assert.Single(result.Tracks);
            Assert.Equal("Active Track", result.Tracks[0].Title);
        }

        // ==================== CreateCareerPathAsync ====================

        [Fact]
        public async Task CreateCareerPathAsync_HappyPath_ReturnsSummary()
        {
            _repoMock.Setup(r => r.AddCareerPathAsync(It.IsAny<CareerPath>())).Returns(Task.CompletedTask);

            var result = await _service.CreateCareerPathAsync(
                new CreateCareerPathDto { Title = "Engineering" }, AdminId);

            Assert.Equal("Engineering", result.Title);
            _repoMock.Verify(r => r.AddCareerPathAsync(It.IsAny<CareerPath>()), Times.Once);
        }

        [Fact]
        public async Task CreateCareerPathAsync_DuplicateTitle_Throws()
        {
            MakeCareerPath("Engineering");

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CreateCareerPathAsync(new CreateCareerPathDto { Title = "Engineering" }, AdminId));
        }

        [Fact]
        public async Task CreateCareerPathAsync_DuplicateTitleCaseInsensitive_Throws()
        {
            MakeCareerPath("Engineering");

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CreateCareerPathAsync(new CreateCareerPathDto { Title = "ENGINEERING" }, AdminId));
            Assert.Contains("title_duplicate", ex.Message);
        }

        // ==================== UpdateCareerPathAsync ====================

        [Fact]
        public async Task UpdateCareerPathAsync_NotFound_Throws()
        {
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.UpdateCareerPathAsync(Guid.NewGuid(), new UpdateCareerPathDto { Title = "X" }, AdminId));
        }

        [Fact]
        public async Task UpdateCareerPathAsync_DuplicateTitleOnOtherPath_Throws()
        {
            var existing = MakeCareerPath("Engineering");
            MakeCareerPath("Design");

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.UpdateCareerPathAsync(existing.Id, new UpdateCareerPathDto { Title = "Design" }, AdminId));
            Assert.Contains("title_duplicate", ex.Message);
        }

        [Fact]
        public async Task UpdateCareerPathAsync_SameTitleSameEntity_DoesNotThrow()
        {
            var cp = MakeCareerPath("Engineering");

            var result = await _service.UpdateCareerPathAsync(cp.Id, new UpdateCareerPathDto { Title = "Engineering" }, AdminId);
            Assert.Equal("Engineering", result.Title);
        }

        // ==================== GetCareerTracksAsync ====================

        [Fact]
        public async Task GetCareerTracksAsync_SoftDeletedTracksFiltered()
        {
            var cp = MakeCareerPath();
            MakeCareerTrack(cp.Id, "Active Track");
            MakeCareerTrack(cp.Id, "Deleted Track", deleted: true);

            var result = await _service.GetCareerTracksAsync(null, 1, 20, "title", "asc");
            Assert.Equal(1, result.TotalItems);
        }

        [Fact]
        public async Task GetCareerTracksAsync_TrackWithDeletedCareerPath_Excluded()
        {
            var activecp = MakeCareerPath("Active");
            var deletedcp = MakeCareerPath("Deleted", deleted: true);
            MakeCareerTrack(activecp.Id, "Track A");
            MakeCareerTrack(deletedcp.Id, "Track B");

            var result = await _service.GetCareerTracksAsync(null, 1, 20, "title", "asc");
            Assert.Equal(1, result.TotalItems);
            Assert.Equal("Track A", result.Data[0].Title);
        }

        [Fact]
        public async Task GetCareerTracksAsync_FilterByCareerPathId_ReturnsOnlyMatchingTracks()
        {
            var cp1 = MakeCareerPath("Engineering");
            var cp2 = MakeCareerPath("Design");
            MakeCareerTrack(cp1.Id, "Backend");
            MakeCareerTrack(cp1.Id, "Frontend");
            MakeCareerTrack(cp2.Id, "UX");

            var result = await _service.GetCareerTracksAsync(cp1.Id, 1, 20, "title", "asc");
            Assert.Equal(2, result.TotalItems);
        }

        // ==================== GetCareerTrackByIdAsync ====================

        [Fact]
        public async Task GetCareerTrackByIdAsync_NotFound_ReturnsNull()
        {
            var result = await _service.GetCareerTrackByIdAsync(Guid.NewGuid());
            Assert.Null(result);
        }

        [Fact]
        public async Task GetCareerTrackByIdAsync_IncludesPositionsOrdered()
        {
            var cp = MakeCareerPath();
            var ct = MakeCareerTrack(cp.Id);
            MakePosition(ct.Id, "Senior", sortOrder: 2);
            MakePosition(ct.Id, "Junior", sortOrder: 1);
            MakePosition(ct.Id, "Deleted", sortOrder: 0, deleted: true);

            var result = await _service.GetCareerTrackByIdAsync(ct.Id);
            Assert.NotNull(result);
            Assert.Equal(2, result.Positions.Count);
            Assert.Equal("Junior", result.Positions[0].Title);
            Assert.Equal("Senior", result.Positions[1].Title);
        }

        // ==================== CreateCareerTrackAsync ====================

        [Fact]
        public async Task CreateCareerTrackAsync_HappyPath_ReturnsSummary()
        {
            var cp = MakeCareerPath("Engineering");
            _repoMock.Setup(r => r.AddCareerTrackAsync(It.IsAny<CareerTrack>())).Returns(Task.CompletedTask);

            var result = await _service.CreateCareerTrackAsync(
                new CreateCareerTrackDto { Title = "Backend", CareerPathId = cp.Id }, AdminId);

            Assert.Equal("Backend", result.Title);
            Assert.Equal("Engineering", result.CareerPathTitle);
        }

        [Fact]
        public async Task CreateCareerTrackAsync_DeletedCareerPath_Throws()
        {
            var cp = MakeCareerPath(deleted: true);

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.CreateCareerTrackAsync(
                    new CreateCareerTrackDto { Title = "Backend", CareerPathId = cp.Id }, AdminId));
            Assert.Contains("career_paths.not_found", ex.Message);
        }

        // ==================== GetPositionByIdAsync ====================

        [Fact]
        public async Task GetPositionByIdAsync_NotFound_ReturnsNull()
        {
            var result = await _service.GetPositionByIdAsync(Guid.NewGuid());
            Assert.Null(result);
        }

        [Fact]
        public async Task GetPositionByIdAsync_WithActiveSkills_ReturnsSkills()
        {
            var cp = MakeCareerPath();
            var ct = MakeCareerTrack(cp.Id);
            var pos = MakePosition(ct.Id);
            var cat = MakeSkillCategory();
            var skill = MakeSkill(cat.Id);
            var level = MakeSkillLevel(skill.Id);
            MakePositionSkill(pos.Id, skill.Id, level.Id);

            var result = await _service.GetPositionByIdAsync(pos.Id);
            Assert.NotNull(result);
            Assert.Single(result.Skills);
            Assert.Equal(skill.Title, result.Skills[0].SkillTitle);
        }

        [Fact]
        public async Task GetPositionByIdAsync_DeletedSkillRequirement_Excluded()
        {
            var cp = MakeCareerPath();
            var ct = MakeCareerTrack(cp.Id);
            var pos = MakePosition(ct.Id);
            var cat = MakeSkillCategory();
            var skill = MakeSkill(cat.Id);
            var level = MakeSkillLevel(skill.Id);
            MakePositionSkill(pos.Id, skill.Id, level.Id, deleted: true);

            var result = await _service.GetPositionByIdAsync(pos.Id);
            Assert.NotNull(result);
            Assert.Empty(result.Skills);
        }

        // ==================== CreatePositionAsync ====================

        [Fact]
        public async Task CreatePositionAsync_HappyPath_ReturnsSummary()
        {
            var cp = MakeCareerPath();
            var ct = MakeCareerTrack(cp.Id, "Backend");
            _repoMock.Setup(r => r.AddPositionAsync(It.IsAny<Position>())).Returns(Task.CompletedTask);

            var result = await _service.CreatePositionAsync(
                new CreatePositionDto { Title = "Senior Dev", CareerTrackId = ct.Id }, AdminId);

            Assert.Equal("Senior Dev", result.Title);
            Assert.Equal("Backend", result.CareerTrackTitle);
        }

        [Fact]
        public async Task CreatePositionAsync_DeletedCareerTrack_Throws()
        {
            var cp = MakeCareerPath();
            var ct = MakeCareerTrack(cp.Id, deleted: true);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.CreatePositionAsync(
                    new CreatePositionDto { Title = "Dev", CareerTrackId = ct.Id }, AdminId));
        }

        // ==================== GetSkillCategoriesAsync ====================

        [Fact]
        public async Task GetSkillCategoriesAsync_SoftDeletedFiltered()
        {
            MakeSkillCategory("Technical");
            MakeSkillCategory("Deleted", deleted: true);

            var result = await _service.GetSkillCategoriesAsync(1, 20, "title", "asc");
            Assert.Equal(1, result.TotalItems);
        }

        // ==================== CreateSkillCategoryAsync ====================

        [Fact]
        public async Task CreateSkillCategoryAsync_HappyPath_ReturnsSummary()
        {
            _repoMock.Setup(r => r.AddSkillCategoryAsync(It.IsAny<SkillCategory>())).Returns(Task.CompletedTask);

            var result = await _service.CreateSkillCategoryAsync(
                new CreateSkillCategoryDto { Title = "Technical" }, AdminId);

            Assert.Equal("Technical", result.Title);
        }

        [Fact]
        public async Task CreateSkillCategoryAsync_DuplicateTitle_Throws()
        {
            MakeSkillCategory("Technical");

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CreateSkillCategoryAsync(new CreateSkillCategoryDto { Title = "technical" }, AdminId));
            Assert.Contains("title_duplicate", ex.Message);
        }

        // ==================== CreateSkillAsync ====================

        [Fact]
        public async Task CreateSkillAsync_HappyPath_ReturnsDetail()
        {
            var cat = MakeSkillCategory("Technical");
            _repoMock.Setup(r => r.AddSkillAsync(It.IsAny<Skill>())).Returns(Task.CompletedTask);

            var result = await _service.CreateSkillAsync(
                new CreateSkillDto { Title = "C#", CategoryId = cat.Id }, AdminId);

            Assert.Equal("C#", result.Title);
            Assert.Equal("Technical", result.CategoryTitle);
        }

        [Fact]
        public async Task CreateSkillAsync_DeletedCategory_Throws()
        {
            var cat = MakeSkillCategory(deleted: true);

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.CreateSkillAsync(new CreateSkillDto { Title = "C#", CategoryId = cat.Id }, AdminId));
            Assert.Contains("skill_categories.not_found", ex.Message);
        }

        // ==================== DeleteSkillAsync ====================

        [Fact]
        public async Task DeleteSkillAsync_HappyPath_SetsIsDeleted()
        {
            var cat = MakeSkillCategory();
            var skill = MakeSkill(cat.Id);

            await _service.DeleteSkillAsync(skill.Id, AdminId);

            var entity = await _db.Skills.FindAsync(skill.Id);
            Assert.True(entity!.IsDeleted);
            Assert.Equal(AdminId, entity.DeletedBy);
        }

        [Fact]
        public async Task DeleteSkillAsync_NotFound_Throws()
        {
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.DeleteSkillAsync(Guid.NewGuid(), AdminId));
        }

        // ==================== AddSkillLevelAsync ====================

        [Fact]
        public async Task AddSkillLevelAsync_HappyPath_ReturnsSummary()
        {
            var cat = MakeSkillCategory();
            var skill = MakeSkill(cat.Id);
            _repoMock.Setup(r => r.AddSkillLevelAsync(It.IsAny<SkillLevel>())).Returns(Task.CompletedTask);

            var result = await _service.AddSkillLevelAsync(
                skill.Id, new AddSkillLevelDto { Value = 3, Title = "Intermediate" }, AdminId);

            Assert.Equal(3, result.Value);
            Assert.Equal("Intermediate", result.Title);
        }

        [Fact]
        public async Task AddSkillLevelAsync_DuplicateValue_Throws()
        {
            var cat = MakeSkillCategory();
            var skill = MakeSkill(cat.Id);
            MakeSkillLevel(skill.Id, value: 3);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.AddSkillLevelAsync(
                    skill.Id, new AddSkillLevelDto { Value = 3, Title = "Another" }, AdminId));
            Assert.Contains("value_duplicate", ex.Message);
        }

        [Fact]
        public async Task AddSkillLevelAsync_ValueOutOfRange_Throws()
        {
            var cat = MakeSkillCategory();
            var skill = MakeSkill(cat.Id);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.AddSkillLevelAsync(
                    skill.Id, new AddSkillLevelDto { Value = 6, Title = "Invalid" }, AdminId));
            Assert.Contains("value_out_of_range", ex.Message);
        }

        [Fact]
        public async Task AddSkillLevelAsync_SkillNotFound_Throws()
        {
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.AddSkillLevelAsync(
                    Guid.NewGuid(), new AddSkillLevelDto { Value = 1, Title = "Beginner" }, AdminId));
        }

        // ==================== AddPositionSkillAsync ====================

        [Fact]
        public async Task AddPositionSkillAsync_HappyPath_ReturnsRequirement()
        {
            var cp = MakeCareerPath();
            var ct = MakeCareerTrack(cp.Id);
            var pos = MakePosition(ct.Id);
            var cat = MakeSkillCategory();
            var skill = MakeSkill(cat.Id);
            var level = MakeSkillLevel(skill.Id, value: 2);
            _repoMock.Setup(r => r.AddPositionToSkillAsync(It.IsAny<PositionToSkill>())).Returns(Task.CompletedTask);

            var result = await _service.AddPositionSkillAsync(
                pos.Id,
                new AddPositionSkillDto { SkillId = skill.Id, SkillLevelId = level.Id, IsMandatory = true },
                AdminId);

            Assert.Equal(skill.Id, result.SkillId);
            Assert.Equal(level.Id, result.SkillLevelId);
            Assert.Equal(2, result.SkillLevelValue);
        }

        [Fact]
        public async Task AddPositionSkillAsync_SkillLevelBelongsToDifferentSkill_Throws()
        {
            var cp = MakeCareerPath();
            var ct = MakeCareerTrack(cp.Id);
            var pos = MakePosition(ct.Id);
            var cat = MakeSkillCategory();
            var skill1 = MakeSkill(cat.Id, "Skill1");
            var skill2 = MakeSkill(cat.Id, "Skill2");
            var levelForSkill2 = MakeSkillLevel(skill2.Id, value: 1);
            _repoMock.Setup(r => r.AddPositionToSkillAsync(It.IsAny<PositionToSkill>())).Returns(Task.CompletedTask);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.AddPositionSkillAsync(
                    pos.Id,
                    new AddPositionSkillDto { SkillId = skill1.Id, SkillLevelId = levelForSkill2.Id, IsMandatory = false },
                    AdminId));
            Assert.Contains("not_belong_to_skill", ex.Message);
        }

        [Fact]
        public async Task AddPositionSkillAsync_DuplicateSkill_Throws()
        {
            var cp = MakeCareerPath();
            var ct = MakeCareerTrack(cp.Id);
            var pos = MakePosition(ct.Id);
            var cat = MakeSkillCategory();
            var skill = MakeSkill(cat.Id);
            var level = MakeSkillLevel(skill.Id);
            MakePositionSkill(pos.Id, skill.Id, level.Id);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.AddPositionSkillAsync(
                    pos.Id,
                    new AddPositionSkillDto { SkillId = skill.Id, SkillLevelId = level.Id, IsMandatory = false },
                    AdminId));
            Assert.Contains("duplicate", ex.Message);
        }

        [Fact]
        public async Task AddPositionSkillAsync_DeletedSkill_Throws()
        {
            var cp = MakeCareerPath();
            var ct = MakeCareerTrack(cp.Id);
            var pos = MakePosition(ct.Id);
            var cat = MakeSkillCategory();
            var skill = MakeSkill(cat.Id, deleted: true);
            var level = new SkillLevel { Id = Guid.NewGuid(), Title = "L", Value = 1, SkillId = skill.Id, CreatedAt = DateTimeOffset.UtcNow };
            _db.SkillLevels.Add(level);
            await _db.SaveChangesAsync();

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.AddPositionSkillAsync(
                    pos.Id,
                    new AddPositionSkillDto { SkillId = skill.Id, SkillLevelId = level.Id, IsMandatory = false },
                    AdminId));
        }

        // ==================== UpdatePositionSkillAsync ====================

        [Fact]
        public async Task UpdatePositionSkillAsync_SkillLevelBelongsToDifferentSkill_Throws()
        {
            var cp = MakeCareerPath();
            var ct = MakeCareerTrack(cp.Id);
            var pos = MakePosition(ct.Id);
            var cat = MakeSkillCategory();
            var skill1 = MakeSkill(cat.Id, "Skill1");
            var skill2 = MakeSkill(cat.Id, "Skill2");
            var level1 = MakeSkillLevel(skill1.Id, value: 1);
            var level2 = MakeSkillLevel(skill2.Id, value: 1);
            var pts = MakePositionSkill(pos.Id, skill1.Id, level1.Id);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.UpdatePositionSkillAsync(
                    pos.Id, pts.Id,
                    new UpdatePositionSkillDto { SkillLevelId = level2.Id },
                    AdminId));
            Assert.Contains("not_belong_to_skill", ex.Message);
        }

        // ==================== GetSkillsAsync ====================

        [Fact]
        public async Task GetSkillsAsync_SoftDeletedSkillsFiltered()
        {
            var cat = MakeSkillCategory();
            MakeSkill(cat.Id, "Active");
            MakeSkill(cat.Id, "Deleted", deleted: true);

            var result = await _service.GetSkillsAsync(null, 1, 20, "title", "asc");
            Assert.Equal(1, result.TotalItems);
        }

        [Fact]
        public async Task GetSkillsAsync_FilterByCategoryId_ReturnsOnlyMatchingSkills()
        {
            var cat1 = MakeSkillCategory("Technical");
            var cat2 = MakeSkillCategory("Soft Skills");
            MakeSkill(cat1.Id, "C#");
            MakeSkill(cat1.Id, "SQL");
            MakeSkill(cat2.Id, "Communication");

            var result = await _service.GetSkillsAsync(cat1.Id, 1, 20, "title", "asc");
            Assert.Equal(2, result.TotalItems);
        }

        // ==================== GetSkillByIdAsync ====================

        [Fact]
        public async Task GetSkillByIdAsync_NotFound_ReturnsNull()
        {
            var result = await _service.GetSkillByIdAsync(Guid.NewGuid());
            Assert.Null(result);
        }

        [Fact]
        public async Task GetSkillByIdAsync_IncludesActiveLevels()
        {
            var cat = MakeSkillCategory();
            var skill = MakeSkill(cat.Id);
            MakeSkillLevel(skill.Id, value: 1, title: "Beginner");
            MakeSkillLevel(skill.Id, value: 3, title: "Intermediate");
            MakeSkillLevel(skill.Id, value: 5, title: "Expert", deleted: true);

            var result = await _service.GetSkillByIdAsync(skill.Id);
            Assert.NotNull(result);
            Assert.Equal(2, result.Levels.Count);
            Assert.Equal(1, result.Levels[0].Value); // ordered by value
        }

        // ==================== Pagination ====================

        [Fact]
        public async Task GetCareerPathsAsync_Pagination_ReturnsCorrectPage()
        {
            for (var i = 1; i <= 5; i++)
                MakeCareerPath($"Path {i:D2}");

            var result = await _service.GetCareerPathsAsync(2, 2, "title", "asc");
            Assert.Equal(5, result.TotalItems);
            Assert.Equal(2, result.Data.Length);
            Assert.Equal(2, result.Page);
            Assert.Equal(3, result.TotalPages);
        }
    }
}
