using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CPR.Domain.Entities;
using CPR.Application.Repositories;

namespace CPR.Infrastructure.Repositories
{
    public class UserRoleRepository : IUserRoleRepository
    {
        private readonly CPR.Infrastructure.Data.CprDbContext _db;

        public UserRoleRepository(CPR.Infrastructure.Data.CprDbContext db)
        {
            _db = db;
        }

        public async Task<UserToRole?> GetByIdAsync(Guid id)
        {
            return await _db.UserRoles.FindAsync(id).AsTask();
        }

        public Task<IQueryable<UserToRole>> GetUserRolesAsync(Guid userId)
        {
            return Task.FromResult(_db.UserRoles
                .Where(ur => ur.UserId == userId && !ur.IsDeleted)
                .AsQueryable());
        }

        public async Task<bool> UserHasRoleAsync(Guid userId, Guid roleId)
        {
            return await _db.UserRoles
                .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId && !ur.IsDeleted);
        }

        public async Task<bool> UserHasRoleAsync(Guid userId, string roleTitle)
        {
            return await _db.UserRoles
                .Include(ur => ur.Role)
                .AnyAsync(ur => ur.UserId == userId &&
                               ur.Role.Title == roleTitle &&
                               !ur.IsDeleted &&
                               !ur.Role.IsDeleted);
        }

        public async Task AddAsync(UserToRole userRole)
        {
            await _db.UserRoles.AddAsync(userRole);
            await _db.SaveChangesAsync();
        }

        public async Task RemoveAsync(UserToRole userRole)
        {
            userRole.IsDeleted = true;
            userRole.DeletedAt = DateTimeOffset.UtcNow;
            _db.UserRoles.Update(userRole);
            await _db.SaveChangesAsync();
        }

        public IQueryable<UserToRole> QueryActive()
        {
            return _db.UserRoles.Where(ur => !ur.IsDeleted);
        }
    }
}