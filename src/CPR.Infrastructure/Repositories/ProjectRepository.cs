using System;
using System.Linq;
using System.Threading.Tasks;
using CPR.Domain.Entities;
using CPR.Application.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CPR.Infrastructure.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly CPR.Infrastructure.Data.CprDbContext _db;

        public ProjectRepository(CPR.Infrastructure.Data.CprDbContext db)
        {
            _db = db;
        }

        // Project operations
        public async Task<Project?> GetByIdAsync(Guid id, bool includeDeleted = false)
        {
            var query = _db.Projects.AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(p => !p.IsDeleted);
            }

            return await query.FirstOrDefaultAsync(p => p.Id == id);
        }

        public IQueryable<Project> QueryAll(bool includeDeleted = false)
        {
            var query = _db.Projects.AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(p => !p.IsDeleted);
            }

            return query;
        }

        public async Task AddAsync(Project project)
        {
            await _db.Projects.AddAsync(project);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Project project)
        {
            _db.Projects.Update(project);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Project project)
        {
            project.IsDeleted = true;
            await UpdateAsync(project);
        }

        // ProjectRole operations
        public async Task<ProjectRole?> GetRoleByIdAsync(Guid id, bool includeDeleted = false)
        {
            var query = _db.ProjectRoles.AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(pr => !pr.IsDeleted);
            }

            return await query.FirstOrDefaultAsync(pr => pr.Id == id);
        }

        public IQueryable<ProjectRole> QueryRolesByProjectId(Guid projectId, bool includeDeleted = false)
        {
            // ProjectRoles are linked to Projects through ProjectTeam
            // Get distinct role IDs used in this project
            var roleIdsInProject = _db.ProjectTeams
                .Where(pt => pt.ProjectId == projectId && !pt.IsDeleted)
                .Select(pt => pt.ProjectRoleId)
                .Distinct();

            var query = _db.ProjectRoles.Where(pr => roleIdsInProject.Contains(pr.Id));

            if (!includeDeleted)
            {
                query = query.Where(pr => !pr.IsDeleted);
            }

            return query;
        }

        public async Task AddRoleAsync(ProjectRole projectRole)
        {
            await _db.ProjectRoles.AddAsync(projectRole);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateRoleAsync(ProjectRole projectRole)
        {
            _db.ProjectRoles.Update(projectRole);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteRoleAsync(ProjectRole projectRole)
        {
            projectRole.IsDeleted = true;
            await UpdateRoleAsync(projectRole);
        }

        // ProjectTeam operations
        public async Task<ProjectTeam?> GetTeamMemberByIdAsync(Guid id, bool includeDeleted = false)
        {
            var query = _db.ProjectTeams.AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(pt => !pt.IsDeleted);
            }

            return await query.FirstOrDefaultAsync(pt => pt.Id == id);
        }

        public IQueryable<ProjectTeam> QueryTeamByProjectId(Guid projectId, bool includeDeleted = false)
        {
            var query = _db.ProjectTeams.Where(pt => pt.ProjectId == projectId);

            if (!includeDeleted)
            {
                query = query.Where(pt => !pt.IsDeleted);
            }

            return query;
        }

        public async Task AddTeamMemberAsync(ProjectTeam projectTeam)
        {
            await _db.ProjectTeams.AddAsync(projectTeam);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteTeamMemberAsync(ProjectTeam projectTeam)
        {
            projectTeam.IsDeleted = true;
            _db.ProjectTeams.Update(projectTeam);
            await _db.SaveChangesAsync();
        }
    }
}
