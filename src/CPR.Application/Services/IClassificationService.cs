using System;
using System.Threading.Tasks;
using CPR.Application.Contracts;

namespace CPR.Application.Services
{
    public interface IClassificationService
    {
        Task<CareerPathDto[]> GetCareerPathsAsync();
        Task<CareerTrackDto[]> GetCareerTracksAsync(Guid? careerPathId = null);
        Task<PositionDto[]> GetPositionsAsync(Guid? careerTrackId = null);
        Task<SkillDto[]> GetSkillsAsync(Guid? positionId = null);
        Task<SkillLevelDto[]> GetSkillLevelsAsync(Guid? skillId = null);
    }
}
