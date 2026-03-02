using CPR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CPR.Infrastructure.Data.Configurations
{
    public class ReviewResponseConfiguration : IEntityTypeConfiguration<ReviewResponse>
    {
        public void Configure(EntityTypeBuilder<ReviewResponse> builder)
        {
            builder.ToTable("review_responses");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.CycleId).HasColumnName("cycle_id");
            builder.Property(x => x.NomineeId).HasColumnName("nominee_id");
            builder.Property(x => x.ReviewerEmployeeId).HasColumnName("reviewer_employee_id");
            builder.Property(x => x.OverallRating).HasColumnName("overall_rating").HasColumnType("smallint");
            builder.Property(x => x.Comments).HasColumnName("comments").IsRequired();
            builder.Property(x => x.CreatedBy).HasColumnName("created_by");
            builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.ModifiedBy).HasColumnName("modified_by");
            builder.Property(x => x.ModifiedAt).HasColumnName("modified_at");
            builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
            builder.Property(x => x.DeletedBy).HasColumnName("deleted_by");
            builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

            builder.HasOne(x => x.Cycle)
                .WithMany()
                .HasForeignKey(x => x.CycleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Nominee)
                .WithOne(x => x.Response)
                .HasForeignKey<ReviewResponse>(x => x.NomineeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ReviewerEmployee)
                .WithMany()
                .HasForeignKey(x => x.ReviewerEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.NomineeId)
                .HasFilter("is_deleted = FALSE")
                .IsUnique()
                .HasDatabaseName("UX_review_responses_nominee");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
