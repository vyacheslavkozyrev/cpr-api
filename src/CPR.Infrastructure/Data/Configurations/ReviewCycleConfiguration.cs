using CPR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CPR.Infrastructure.Data.Configurations
{
    public class ReviewCycleConfiguration : IEntityTypeConfiguration<ReviewCycle>
    {
        public void Configure(EntityTypeBuilder<ReviewCycle> builder)
        {
            builder.ToTable("review_cycles");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.Title).HasColumnName("title").IsRequired();
            builder.Property(x => x.Description).HasColumnName("description");
            builder.Property(x => x.SubjectEmployeeId).HasColumnName("subject_employee_id");
            builder.Property(x => x.DepartmentId).HasColumnName("department_id");
            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasConversion(
                    v => v.ToString().ToLower(),
                    v => Enum.Parse<ReviewCycleStatus>(v, true));
            builder.Property(x => x.OpenedAt).HasColumnName("opened_at");
            builder.Property(x => x.StartedAt).HasColumnName("started_at");
            builder.Property(x => x.ClosedAt).HasColumnName("closed_at");
            builder.Property(x => x.CreatedBy).HasColumnName("created_by");
            builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.ModifiedBy).HasColumnName("modified_by");
            builder.Property(x => x.ModifiedAt).HasColumnName("modified_at");
            builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
            builder.Property(x => x.DeletedBy).HasColumnName("deleted_by");
            builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

            builder.HasOne(x => x.SubjectEmployee)
                .WithMany()
                .HasForeignKey(x => x.SubjectEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Nominees)
                .WithOne(x => x.Cycle)
                .HasForeignKey(x => x.CycleId);

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
