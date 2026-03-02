using CPR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CPR.Infrastructure.Data.Configurations
{
    public class ReviewNomineeConfiguration : IEntityTypeConfiguration<ReviewNominee>
    {
        public void Configure(EntityTypeBuilder<ReviewNominee> builder)
        {
            builder.ToTable("review_nominees");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.CycleId).HasColumnName("cycle_id");
            builder.Property(x => x.ReviewerEmployeeId).HasColumnName("reviewer_employee_id");
            builder.Property(x => x.NominatedBy).HasColumnName("nominated_by");
            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasConversion(
                    v => v.ToString().ToLower(),
                    v => Enum.Parse<ReviewNomineeStatus>(v, true));
            builder.Property(x => x.CreatedBy).HasColumnName("created_by");
            builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.ModifiedBy).HasColumnName("modified_by");
            builder.Property(x => x.ModifiedAt).HasColumnName("modified_at");
            builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
            builder.Property(x => x.DeletedBy).HasColumnName("deleted_by");
            builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

            builder.HasOne(x => x.ReviewerEmployee)
                .WithMany()
                .HasForeignKey(x => x.ReviewerEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.NominatedByEmployee)
                .WithMany()
                .HasForeignKey(x => x.NominatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.CycleId, x.ReviewerEmployeeId })
                .HasFilter("is_deleted = FALSE")
                .IsUnique()
                .HasDatabaseName("UX_review_nominees_cycle_reviewer");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
