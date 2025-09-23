using System;
using System.Linq;
using System.Threading.Tasks;
using CPR.Infrastructure.Data;
using CPR.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CPR.UnitTests
{
    public class ClassificationServiceTests : IDisposable
    {
        private readonly SqliteConnection _conn;
        private readonly CprDbContext _db;

        public ClassificationServiceTests()
        {
            _conn = new SqliteConnection("DataSource=:memory:");
            _conn.Open();
            var options = new DbContextOptionsBuilder<CprDbContext>()
                .UseSqlite(_conn)
                .Options;

            _db = new CprDbContext(options);
            _db.Database.EnsureDeleted();
            _db.Database.EnsureCreated();

            // seed minimal data
            var pathId = Guid.NewGuid();
            var trackId = Guid.NewGuid();
            var posId = Guid.NewGuid();

            // Add 4 career paths as expected by the test
            _db.CareerPaths.Add(new Domain.Entities.CareerPath { Id = pathId, Title = "Path 1", Description = "pdesc" });
            _db.CareerPaths.Add(new Domain.Entities.CareerPath { Id = Guid.NewGuid(), Title = "Path 2", Description = "pdesc2" });
            _db.CareerPaths.Add(new Domain.Entities.CareerPath { Id = Guid.NewGuid(), Title = "Path 3", Description = "pdesc3" });
            _db.CareerPaths.Add(new Domain.Entities.CareerPath { Id = Guid.NewGuid(), Title = "Path 4", Description = "pdesc4" });

            _db.CareerTracks.Add(new Domain.Entities.CareerTrack { Id = trackId, Title = "Software Engineering", Description = "tdesc", CareerPathId = pathId });
            _db.Positions.Add(new Domain.Entities.Position { Id = posId, Title = "Senior Software Engineer", Description = "odesc", Expectations = null, CareerTrackId = trackId });
            _db.SaveChanges();
        }

        [Fact]
        public async Task GetCareerPaths_ReturnsSeeded()
        {
            var svc = new ClassificationService(_db);
            var res = await svc.GetCareerPathsAsync();
            Assert.Equal(4, res.Count());
            Assert.Contains(res, p => p.Title == "Path 1");
        }

        [Fact]
        public async Task GetCareerTracks_FilteredByPath_ReturnsTrack()
        {
            var svc = new ClassificationService(_db);
            var path = (await svc.GetCareerPathsAsync()).First(p => p.Title == "Path 1");
            var tracks = await svc.GetCareerTracksAsync(path.Id);
            Assert.Single(tracks);
            Assert.Equal("Software Engineering", tracks[0].Title);
        }

        [Fact]
        public async Task GetPositions_FilteredByTrack_ReturnsPosition_WithExpectations()
        {
            var svc = new ClassificationService(_db);
            var paths = await svc.GetCareerPathsAsync();
            var path = paths.First(p => p.Title == "Path 1");
            var tracks = await svc.GetCareerTracksAsync(path.Id);
            var track = tracks.First();
            var positions = await svc.GetPositionsAsync(track.Id);
            Assert.Single(positions);
            Assert.Equal("Senior Software Engineer", positions[0].Title);
            Assert.Null(positions[0].Expectations);
        }

        [Fact]
        public async Task EmptyResults_ReturnsEmptyArrays()
        {
            // clear tables
            _db.Positions.RemoveRange(_db.Positions);
            _db.CareerTracks.RemoveRange(_db.CareerTracks);
            _db.CareerPaths.RemoveRange(_db.CareerPaths);
            _db.SaveChanges();

            var svc = new ClassificationService(_db);
            var cp = await svc.GetCareerPathsAsync();
            var ct = await svc.GetCareerTracksAsync();
            var pos = await svc.GetPositionsAsync();

            Assert.Empty(cp);
            Assert.Empty(ct);
            Assert.Empty(pos);
        }

        [Fact]
        public async Task InvalidGuidFilter_ReturnsEmpty_NoException()
        {
            var svc = new ClassificationService(_db);
            // use a random GUID that doesn't exist
            var res = await svc.GetCareerTracksAsync(Guid.NewGuid());
            Assert.Empty(res);

            var pres = await svc.GetPositionsAsync(Guid.NewGuid());
            Assert.Empty(pres);
        }

        [Fact]
        public async Task DeletedItems_AreFilteredOut()
        {
            // mark existing entities as deleted
            foreach (var p in _db.CareerPaths) p.IsDeleted = true;
            foreach (var t in _db.CareerTracks) t.IsDeleted = true;
            foreach (var p in _db.Positions) p.IsDeleted = true;
            _db.SaveChanges();

            var svc = new ClassificationService(_db);
            Assert.Empty(await svc.GetCareerPathsAsync());
            Assert.Empty(await svc.GetCareerTracksAsync());
            Assert.Empty(await svc.GetPositionsAsync());
        }

        public void Dispose()
        {
            _db?.Dispose();
            _conn?.Dispose();
        }
    }
}
