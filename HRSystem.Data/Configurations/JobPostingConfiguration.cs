using HRSystem.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRSystem.Data.Configurations;

public class JobPostingConfiguration : IEntityTypeConfiguration<JobPosting>
{
    public void Configure(EntityTypeBuilder<JobPosting> entity)
    {
        entity.ToTable("JobPostings");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(x => x.Description)
            .IsRequired();

        entity.Property(x => x.Requirements)
            .IsRequired();

        entity.Property(x => x.Status)
            .IsRequired();

        entity.Property(x => x.ClosingDate)
            .IsRequired();

        entity.Property(x => x.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        entity.HasOne(x => x.Department)
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.CreatedBy)
            .WithMany()
            .HasForeignKey(x => x.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasMany(x => x.Applications)
            .WithOne(x => x.JobPosting)
            .HasForeignKey(x => x.JobPostingId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.Status);
        entity.HasIndex(x => x.ClosingDate);
        entity.HasIndex(x => x.DepartmentId);
    }
}
