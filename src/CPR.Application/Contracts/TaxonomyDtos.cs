using System;
using System.ComponentModel.DataAnnotations;

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

    // ==================== Skill Category DTOs ====================

    /// <summary>
    /// Skill category (grouping of skills)
    /// </summary>
    public class SkillCategoryDto
    {
        /// <summary>Identifier (GUID)</summary>
        public Guid Id { get; set; }
        /// <summary>Human-friendly title</summary>
        public string Title { get; set; } = null!;
        /// <summary>Optional description</summary>
        public string? Description { get; set; }
    }

    /// <summary>
    /// DTO for creating a new skill category
    /// </summary>
    public class CreateSkillCategoryDto
    {
        /// <summary>Human-friendly title (required, 1-250 characters)</summary>
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(250, ErrorMessage = "Title cannot exceed 250 characters")]
        public string Title { get; set; } = null!;

        /// <summary>Optional description (max 2000 characters)</summary>
        [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing skill category (all fields optional for partial updates)
    /// </summary>
    public class UpdateSkillCategoryDto
    {
        /// <summary>Human-friendly title (optional, 1-250 characters)</summary>
        [MaxLength(250, ErrorMessage = "Title cannot exceed 250 characters")]
        public string? Title { get; set; }

        /// <summary>Optional description (max 2000 characters)</summary>
        [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string? Description { get; set; }
    }

    // ==================== Career Path DTOs (Create/Update) ====================

    /// <summary>
    /// DTO for creating a new career path
    /// </summary>
    public class CreateCareerPathDto
    {
        /// <summary>Human-friendly title (required, 1-250 characters)</summary>
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(250, ErrorMessage = "Title cannot exceed 250 characters")]
        public string Title { get; set; } = null!;

        /// <summary>Optional description (max 2000 characters)</summary>
        [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing career path (all fields optional for partial updates)
    /// </summary>
    public class UpdateCareerPathDto
    {
        /// <summary>Human-friendly title (optional, 1-250 characters)</summary>
        [MaxLength(250, ErrorMessage = "Title cannot exceed 250 characters")]
        public string? Title { get; set; }

        /// <summary>Optional description (max 2000 characters)</summary>
        [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string? Description { get; set; }
    }

    // ==================== Career Track DTOs (Create/Update) ====================

    /// <summary>
    /// DTO for creating a new career track
    /// </summary>
    public class CreateCareerTrackDto
    {
        /// <summary>Human-friendly title (required, 1-250 characters)</summary>
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(250, ErrorMessage = "Title cannot exceed 250 characters")]
        public string Title { get; set; } = null!;

        /// <summary>Optional description (max 2000 characters)</summary>
        [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string? Description { get; set; }

        /// <summary>Parent career path identifier (required)</summary>
        [Required(ErrorMessage = "CareerPathId is required")]
        public Guid CareerPathId { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing career track (all fields optional for partial updates)
    /// </summary>
    public class UpdateCareerTrackDto
    {
        /// <summary>Human-friendly title (optional, 1-250 characters)</summary>
        [MaxLength(250, ErrorMessage = "Title cannot exceed 250 characters")]
        public string? Title { get; set; }

        /// <summary>Optional description (max 2000 characters)</summary>
        [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string? Description { get; set; }

        /// <summary>Parent career path identifier (optional)</summary>
        public Guid? CareerPathId { get; set; }
    }

    // ==================== Position DTOs (Create/Update) ====================

    /// <summary>
    /// DTO for creating a new position
    /// </summary>
    public class CreatePositionDto
    {
        /// <summary>Human-friendly title (required, 1-250 characters)</summary>
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(250, ErrorMessage = "Title cannot exceed 250 characters")]
        public string Title { get; set; } = null!;

        /// <summary>Optional description (max 2000 characters)</summary>
        [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string? Description { get; set; }

        /// <summary>Optional expectations text for the position (max 2000 characters)</summary>
        [MaxLength(2000, ErrorMessage = "Expectations cannot exceed 2000 characters")]
        public string? Expectations { get; set; }

        /// <summary>Parent career track identifier (required)</summary>
        [Required(ErrorMessage = "CareerTrackId is required")]
        public Guid CareerTrackId { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing position (all fields optional for partial updates)
    /// </summary>
    public class UpdatePositionDto
    {
        /// <summary>Human-friendly title (optional, 1-250 characters)</summary>
        [MaxLength(250, ErrorMessage = "Title cannot exceed 250 characters")]
        public string? Title { get; set; }

        /// <summary>Optional description (max 2000 characters)</summary>
        [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string? Description { get; set; }

        /// <summary>Optional expectations text for the position (max 2000 characters)</summary>
        [MaxLength(2000, ErrorMessage = "Expectations cannot exceed 2000 characters")]
        public string? Expectations { get; set; }

        /// <summary>Parent career track identifier (optional)</summary>
        public Guid? CareerTrackId { get; set; }
    }

    // ==================== Skill DTOs (Create/Update) ====================

    /// <summary>
    /// DTO for creating a new skill
    /// </summary>
    public class CreateSkillDto
    {
        /// <summary>Human-friendly title (required, 1-250 characters)</summary>
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(250, ErrorMessage = "Title cannot exceed 250 characters")]
        public string Title { get; set; } = null!;

        /// <summary>Optional description (max 2000 characters)</summary>
        [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string? Description { get; set; }

        /// <summary>Parent skill category identifier (required)</summary>
        [Required(ErrorMessage = "CategoryId is required")]
        public Guid CategoryId { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing skill (all fields optional for partial updates)
    /// </summary>
    public class UpdateSkillDto
    {
        /// <summary>Human-friendly title (optional, 1-250 characters)</summary>
        [MaxLength(250, ErrorMessage = "Title cannot exceed 250 characters")]
        public string? Title { get; set; }

        /// <summary>Optional description (max 2000 characters)</summary>
        [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string? Description { get; set; }

        /// <summary>Parent skill category identifier (optional)</summary>
        public Guid? CategoryId { get; set; }
    }

    // ==================== Skill Level DTOs (Create/Update) ====================

    /// <summary>
    /// DTO for creating a new skill level
    /// </summary>
    public class CreateSkillLevelDto
    {
        /// <summary>Human-friendly title (required, 1-250 characters)</summary>
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(250, ErrorMessage = "Title cannot exceed 250 characters")]
        public string Title { get; set; } = null!;

        /// <summary>Optional description (max 2000 characters)</summary>
        [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string? Description { get; set; }

        /// <summary>Numeric value representing the level (1-5, required)</summary>
        [Required(ErrorMessage = "Value is required")]
        [Range(1, 5, ErrorMessage = "Value must be between 1 and 5")]
        public int Value { get; set; }

        /// <summary>Parent skill identifier (required)</summary>
        [Required(ErrorMessage = "SkillId is required")]
        public Guid SkillId { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing skill level (all fields optional for partial updates)
    /// </summary>
    public class UpdateSkillLevelDto
    {
        /// <summary>Human-friendly title (optional, 1-250 characters)</summary>
        [MaxLength(250, ErrorMessage = "Title cannot exceed 250 characters")]
        public string? Title { get; set; }

        /// <summary>Optional description (max 2000 characters)</summary>
        [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string? Description { get; set; }

        /// <summary>Numeric value representing the level (1-5, optional)</summary>
        [Range(1, 5, ErrorMessage = "Value must be between 1 and 5")]
        public int? Value { get; set; }

        /// <summary>Parent skill identifier (optional)</summary>
        public Guid? SkillId { get; set; }
    }

    // ==================== Position-Skill Mapping DTOs ====================

    /// <summary>
    /// DTO for creating a position-to-skill mapping with required skill level
    /// </summary>
    public class CreatePositionSkillMappingDto
    {
        /// <summary>Skill identifier (required)</summary>
        [Required(ErrorMessage = "SkillId is required")]
        public Guid SkillId { get; set; }

        /// <summary>Required skill level identifier (required)</summary>
        [Required(ErrorMessage = "SkillLevelId is required")]
        public Guid SkillLevelId { get; set; }
    }

    /// <summary>
    /// Position-to-skill mapping response with details
    /// </summary>
    public class PositionSkillMappingDto
    {
        /// <summary>Mapping identifier (GUID)</summary>
        public Guid Id { get; set; }

        /// <summary>Position identifier</summary>
        public Guid PositionId { get; set; }

        /// <summary>Skill identifier</summary>
        public Guid SkillId { get; set; }

        /// <summary>Skill title</summary>
        public string SkillTitle { get; set; } = null!;

        /// <summary>Required skill level identifier</summary>
        public Guid SkillLevelId { get; set; }

        /// <summary>Skill level title</summary>
        public string SkillLevelTitle { get; set; } = null!;

        /// <summary>When the mapping was created</summary>
        public DateTimeOffset CreatedAt { get; set; }
    }
}
