using System;

namespace CPR.Application.Contracts
{
    /// <summary>
    /// Career path (top-level grouping of career tracks)
    /// </summary>
    public class CareerPathDto
    {
        /// <summary>Identifier (GUID)</summary>
        public Guid Id { get; set; }
        /// <summary>Human-friendly title</summary>
        public string Title { get; set; } = null!;
        /// <summary>Optional description</summary>
        public string? Description { get; set; }
    }

    /// <summary>
    /// Career track contained by a career path
    /// </summary>
    public class CareerTrackDto
    {
        /// <summary>Identifier (GUID)</summary>
        public Guid Id { get; set; }
        /// <summary>Human-friendly title</summary>
        public string Title { get; set; } = null!;
        /// <summary>Optional description</summary>
        public string? Description { get; set; }
        /// <summary>Parent career path id</summary>
        public Guid CareerPathId { get; set; }
    }

    /// <summary>
    /// Position within a career track
    /// </summary>
    public class PositionDto
    {
        /// <summary>Identifier (GUID)</summary>
        public Guid Id { get; set; }
        /// <summary>Human-friendly title</summary>
        public string Title { get; set; } = null!;
        /// <summary>Optional description</summary>
        public string? Description { get; set; }
        /// <summary>Optional expectations text for the position</summary>
        public string? Expectations { get; set; }
        /// <summary>Parent career track id</summary>
        public Guid CareerTrackId { get; set; }
    }

    /// <summary>
    /// Skill within a category
    /// </summary>
    public class SkillDto
    {
        /// <summary>Identifier (GUID)</summary>
        public Guid Id { get; set; }
        /// <summary>Human-friendly title</summary>
        public string Title { get; set; } = null!;
        /// <summary>Optional description</summary>
        public string? Description { get; set; }
        /// <summary>Parent skill category id</summary>
        public Guid CategoryId { get; set; }
    }

    /// <summary>
    /// Skill level within a skill
    /// </summary>
    public class SkillLevelDto
    {
        /// <summary>Identifier (GUID)</summary>
        public Guid Id { get; set; }
        /// <summary>Human-friendly title</summary>
        public string Title { get; set; } = null!;
        /// <summary>Optional description</summary>
        public string? Description { get; set; }
        /// <summary>Parent skill id</summary>
        public Guid SkillId { get; set; }
        /// <summary>Numeric value representing the level</summary>
        public int Value { get; set; }
    }

    /// <summary>
    /// Employee skill assessment (self-assessment)
    /// </summary>
    public class EmployeeSkillDto
    {
        /// <summary>Identifier (GUID)</summary>
        public Guid Id { get; set; }
        /// <summary>Employee identifier</summary>
        public Guid EmployeeId { get; set; }
        /// <summary>Skill details</summary>
        public SkillDto Skill { get; set; } = null!;
        /// <summary>Current skill level (optional)</summary>
        public SkillLevelDto? CurrentLevel { get; set; }
        /// <summary>Target skill level (optional)</summary>
        public SkillLevelDto? TargetLevel { get; set; }
        /// <summary>Source of the assessment</summary>
        public string? Source { get; set; }
        /// <summary>Effective date of the assessment</summary>
        public DateTimeOffset? EffectiveDate { get; set; }
        /// <summary>Whether this is a target assessment</summary>
        public bool IsTarget { get; set; }
        /// <summary>When the assessment was created</summary>
        public DateTimeOffset CreatedAt { get; set; }
        /// <summary>When the assessment was last modified</summary>
        public DateTimeOffset? ModifiedAt { get; set; }
    }

    /// <summary>
    /// DTO for creating a new employee skill assessment
    /// </summary>
    public class EmployeeSkillCreateDto
    {
        /// <summary>Skill identifier</summary>
        public Guid SkillId { get; set; }
        /// <summary>Current skill level identifier (optional)</summary>
        public Guid? CurrentLevelId { get; set; }
        /// <summary>Target skill level identifier (optional)</summary>
        public Guid? TargetLevelId { get; set; }
        /// <summary>Source of the assessment</summary>
        public string? Source { get; set; }
        /// <summary>Effective date of the assessment</summary>
        public DateTimeOffset? EffectiveDate { get; set; }
        /// <summary>Whether this is a target assessment</summary>
        public bool IsTarget { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing employee skill assessment
    /// </summary>
    public class EmployeeSkillUpdateDto
    {
        /// <summary>Current skill level identifier (optional)</summary>
        public Guid? CurrentLevelId { get; set; }
        /// <summary>Target skill level identifier (optional)</summary>
        public Guid? TargetLevelId { get; set; }
        /// <summary>Source of the assessment</summary>
        public string? Source { get; set; }
        /// <summary>Effective date of the assessment</summary>
        public DateTimeOffset? EffectiveDate { get; set; }
        /// <summary>Whether this is a target assessment</summary>
        public bool IsTarget { get; set; }
    }
}
