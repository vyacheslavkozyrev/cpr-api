using System;
using System.Threading.Tasks;
using CPR.Application.Contracts;

namespace CPR.Application.Services
{
    public interface IClassificationService
    {
        // ==================== READ Operations (existing) ====================
        Task<CareerPathDto[]> GetCareerPathsAsync();
        Task<CareerTrackDto[]> GetCareerTracksAsync(Guid? careerPathId = null);
        Task<PositionDto[]> GetPositionsAsync(Guid? careerTrackId = null);
        Task<SkillCategoryDto[]> GetSkillCategoriesAsync();
        Task<SkillDto[]> GetSkillsAsync(Guid? positionId = null);
        Task<SkillLevelDto[]> GetSkillLevelsAsync(Guid? skillId = null);

        // Employee skill assessment methods
        Task<EmployeeSkillDto[]> GetEmployeeSkillsAsync(Guid employeeId);
        Task<EmployeeSkillDto> CreateEmployeeSkillAsync(Guid employeeId, EmployeeSkillCreateDto dto);
        Task<EmployeeSkillDto> UpdateEmployeeSkillAsync(Guid employeeId, Guid skillId, EmployeeSkillUpdateDto dto);

        // ==================== Career Path CUD Operations ====================
        Task<CareerPathDto> CreateCareerPathAsync(CreateCareerPathDto dto, Guid currentUserId);
        Task<CareerPathDto> UpdateCareerPathAsync(Guid id, UpdateCareerPathDto dto, Guid currentUserId);
        Task DeleteCareerPathAsync(Guid id, Guid currentUserId);

        // ==================== Career Track CUD Operations ====================
        Task<CareerTrackDto> CreateCareerTrackAsync(CreateCareerTrackDto dto, Guid currentUserId);
        Task<CareerTrackDto> UpdateCareerTrackAsync(Guid id, UpdateCareerTrackDto dto, Guid currentUserId);
        Task DeleteCareerTrackAsync(Guid id, Guid currentUserId);

        // ==================== Position CUD Operations ====================
        Task<PositionDto> CreatePositionAsync(CreatePositionDto dto, Guid currentUserId);
        Task<PositionDto> UpdatePositionAsync(Guid id, UpdatePositionDto dto, Guid currentUserId);
        Task DeletePositionAsync(Guid id, Guid currentUserId);

        // ==================== Skill Category CUD Operations ====================
        Task<SkillCategoryDto> CreateSkillCategoryAsync(CreateSkillCategoryDto dto, Guid currentUserId);
        Task<SkillCategoryDto> UpdateSkillCategoryAsync(Guid id, UpdateSkillCategoryDto dto, Guid currentUserId);
        Task DeleteSkillCategoryAsync(Guid id, Guid currentUserId);

        // ==================== Skill CUD Operations ====================
        Task<SkillDto> CreateSkillAsync(CreateSkillDto dto, Guid currentUserId);
        Task<SkillDto> UpdateSkillAsync(Guid id, UpdateSkillDto dto, Guid currentUserId);
        Task DeleteSkillAsync(Guid id, Guid currentUserId);

        // ==================== Skill Level CUD Operations ====================
        Task<SkillLevelDto> CreateSkillLevelAsync(CreateSkillLevelDto dto, Guid currentUserId);
        Task<SkillLevelDto> UpdateSkillLevelAsync(Guid id, UpdateSkillLevelDto dto, Guid currentUserId);
        Task DeleteSkillLevelAsync(Guid id, Guid currentUserId);

        // ==================== Position-Skill Mapping Operations ====================
        Task<PositionSkillMappingDto> AddSkillToPositionAsync(Guid positionId, CreatePositionSkillMappingDto dto, Guid currentUserId);
        Task RemoveSkillFromPositionAsync(Guid positionId, Guid skillId, Guid currentUserId);
        Task<PositionSkillMappingDto[]> GetPositionSkillMappingsAsync(Guid positionId);
    }
}
