using System;
using System.Linq;
using System.Threading.Tasks;
using CPR.Application.Contracts;
using CPR.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace CPR.Infrastructure.Services
{
    public class ClassificationService : IClassificationService
    {
        private readonly CPR.Infrastructure.Data.CprDbContext _db;

        public ClassificationService(CPR.Infrastructure.Data.CprDbContext db)
        {
            _db = db;
        }

        public async Task<CareerPathDto[]> GetCareerPathsAsync()
        {
            var items = await _db.CareerPaths.Where(c => !c.IsDeleted).ToListAsync();
            return items.Select(c => new CareerPathDto { Id = c.Id, Title = c.Title, Description = c.Description }).ToArray();
        }

        public async Task<CareerTrackDto[]> GetCareerTracksAsync(Guid? careerPathId = null)
        {
            var q = _db.CareerTracks.Where(ct => !ct.IsDeleted);
            if (careerPathId.HasValue) q = q.Where(ct => ct.CareerPathId == careerPathId.Value);
            var items = await q.ToListAsync();
            return items.Select(c => new CareerTrackDto { Id = c.Id, Title = c.Title, Description = c.Description, CareerPathId = c.CareerPathId }).ToArray();
        }

        public async Task<PositionDto[]> GetPositionsAsync(Guid? careerTrackId = null)
        {
            var q = _db.Positions.Where(p => !p.IsDeleted);
            if (careerTrackId.HasValue) q = q.Where(p => p.CareerTrackId == careerTrackId.Value);
            var items = await q.ToListAsync();
            return items.Select(p => new PositionDto { Id = p.Id, Title = p.Title, Description = p.Description, Expectations = p.Expectations, CareerTrackId = p.CareerTrackId }).ToArray();
        }
    }
}
