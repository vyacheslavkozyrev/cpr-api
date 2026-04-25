using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using CPR.Application.Services;
using CPR.Application.Repositories;
using CPR.Domain.Entities;
using CPR.Infrastructure.Data;
using CPR.Infrastructure.Repositories;
using CPR.Infrastructure.Services;

namespace CPR.UnitTests.Services
{
    /// <summary>
    /// Unit tests for TeamService.GetDirectReportsAsync (F0010a) —
    /// uses in-memory EF + real TeamRepository to exercise Include chains.
    /// </summary>
    public class TeamServiceDirectReportsTests : IDisposable
    {
        private readonly CprDbContext _db;
        private readonly TeamService _service;

        private static readonly Guid ManagerUserId   = Guid.Parse("aa000000-0000-0000-0000-000000000001");
        private static readonly Guid ManagerEmpId    = Guid.Parse("aa000000-0000-0000-0000-000000000002");
        private static readonly Guid ReportUserId    = Guid.Parse("bb000000-0000-0000-0000-000000000001");
        private static readonly Guid ReportEmpId     = Guid.Parse("bb000000-0000-0000-0000-000000000002");
        private static readonly Guid PositionId      = Guid.Parse("cc000000-0000-0000-0000-000000000001");

        public TeamServiceDirectReportsTests()
        {
            var options = new DbContextOptionsBuilder<CprDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new CprDbContext(options);

            // Seed a career track and position (required FK for Employee.PositionId)
            var track = new CareerTrack { Id = Guid.NewGuid(), Title = "Tech", IsDeleted = false };
            var position = new Position { Id = PositionId, Title = "Developer", CareerTrackId = track.Id, IsDeleted = false };
            _db.CareerTracks.Add(track);
            _db.Positions.Add(position);

            // Manager user + employee
            _db.Users.Add(new User { Id = ManagerUserId, UserName = "manager", DisplayName = "Manager User", IsDeleted = false });
            _db.Employees.Add(new Employee { Id = ManagerEmpId, UserId = ManagerUserId, PositionId = PositionId, IsDeleted = false });

            // Direct report user + employee
            _db.Users.Add(new User { Id = ReportUserId, UserName = "report", DisplayName = "Report User", IsDeleted = false });
            _db.Employees.Add(new Employee { Id = ReportEmpId, UserId = ReportUserId, PositionId = PositionId, ManagerId = ManagerEmpId, IsDeleted = false });

            _db.SaveChanges();

            var teamRepo = new TeamRepository(_db);
            var classificationMock = new Mock<IClassificationService>();
            _service = new TeamService(_db, teamRepo, classificationMock.Object);
        }

        public void Dispose() => _db.Dispose();

        [Fact]
        public async Task GetDirectReportsAsync_Manager_ReturnsDirectReports()
        {
            var reports = await _service.GetDirectReportsAsync(ManagerEmpId);

            Assert.Single(reports);
            Assert.Equal(ReportEmpId, reports[0].Id);
            Assert.Equal("Report User", reports[0].FullName);
        }

        [Fact]
        public async Task GetDirectReportsAsync_ManagerWithNoReports_ReturnsEmptyArray()
        {
            // Use the report employee as manager — they have no direct reports
            var reports = await _service.GetDirectReportsAsync(ReportEmpId);

            Assert.Empty(reports);
        }

        [Fact]
        public async Task GetDirectReportsAsync_UnknownManagerId_ReturnsEmptyArray()
        {
            var reports = await _service.GetDirectReportsAsync(Guid.NewGuid());

            Assert.Empty(reports);
        }
    }
}
