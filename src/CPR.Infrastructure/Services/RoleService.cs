using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CPR.Application.Repositories;
using CPR.Application.Services;
using CPR.Domain.Entities;

namespace CPR.Infrastructure.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRoleRepository _userRoleRepository;

        public RoleService(IRoleRepository roleRepository, IUserRoleRepository userRoleRepository)
        {
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
        }

        public async Task<bool> UserHasRoleAsync(Guid userId, string roleTitle)
        {
            return await _userRoleRepository.UserHasRoleAsync(userId, roleTitle);
        }

        public async Task<bool> UserHasAnyRoleAsync(Guid userId, params string[] roleTitles)
        {
            foreach (var roleTitle in roleTitles)
            {
                if (await UserHasRoleAsync(userId, roleTitle))
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<IEnumerable<string>> GetUserRoleTitlesAsync(Guid userId)
        {
            var userRoles = await _userRoleRepository.GetUserRolesAsync(userId);
            var roleTitles = await userRoles
                .Include(ur => ur.Role)
                .Where(ur => !ur.Role.IsDeleted)
                .Select(ur => ur.Role.Title)
                .ToListAsync();

            return roleTitles;
        }

        public async Task AssignRoleToUserAsync(Guid userId, string roleTitle, Guid assignedBy)
        {
            // Check if role exists
            var role = await _roleRepository.GetByTitleAsync(roleTitle);
            if (role == null)
            {
                throw new ArgumentException($"Role '{roleTitle}' does not exist", nameof(roleTitle));
            }

            // Check if user already has this role
            if (await _userRoleRepository.UserHasRoleAsync(userId, role.Id))
            {
                return; // Already assigned
            }

            // Create the assignment
            var userRole = new UserToRole
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RoleId = role.Id,
                CreatedBy = assignedBy,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _userRoleRepository.AddAsync(userRole);
        }

        public async Task RemoveRoleFromUserAsync(Guid userId, string roleTitle, Guid removedBy)
        {
            // Find the user-role assignment
            var userRoles = await _userRoleRepository.GetUserRolesAsync(userId);
            var userRole = await userRoles
                .Include(ur => ur.Role)
                .FirstOrDefaultAsync(ur => ur.Role.Title == roleTitle);

            if (userRole == null)
            {
                return; // Not assigned
            }

            // Soft delete the assignment
            userRole.DeletedBy = removedBy;
            await _userRoleRepository.RemoveAsync(userRole);
        }

        public async Task<IEnumerable<string>> GetAllRoleTitlesAsync()
        {
            return await _roleRepository.QueryActive()
                .Select(r => r.Title)
                .ToListAsync();
        }
    }
}