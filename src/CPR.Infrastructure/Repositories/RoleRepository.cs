using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CPR.Domain.Entities;
using CPR.Application.Repositories;

namespace CPR.Infrastructure.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly CPR.Infrastructure.Data.CprDbContext _db;

        public RoleRepository(CPR.Infrastructure.Data.CprDbContext db)
        {
            _db = db;
        }

        public async Task<Role?> GetByIdAsync(Guid id)
        {
            return await _db.Roles.FindAsync(id).AsTask();
        }

        public async Task<Role?> GetByTitleAsync(string title)
        {
            return await _db.Roles
                .Where(r => r.Title == title && !r.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public IQueryable<Role> QueryActive()
        {
            return _db.Roles.Where(r => !r.IsDeleted);
        }

        public async Task AddAsync(Role role)
        {
            await _db.Roles.AddAsync(role);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Role role)
        {
            _db.Roles.Update(role);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Role role)
        {
            role.IsDeleted = true;
            role.DeletedAt = DateTimeOffset.UtcNow;
            await UpdateAsync(role);
        }
    }
}