using HRSystem.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRSystem.Data.Configurations;

public class JobApplicationConfiguration : IEntityTypeConfiguration<JobApplication>
{
    public void Configure(EntityTypeBuilder<JobApplication> entity)
    {
        entity.ToTable("JobApplications");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.FullName)
            .IsRequired()
            .HasMaxLength(150);

        entity.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(256);

        entity.Property(x => x.PhoneNumber)
            .IsRequired()
            .HasMaxLength(30);

        entity.Property(x => x.CvPublicId)
            .IsRequired()
            .HasMaxLength(255);

        entity.Property(x => x.CvOriginalFileName)
            .IsRequired()
            .HasMaxLength(255);

        entity.Property(x => x.CvStorageUrl)
            .IsRequired()
            .HasMaxLength(1000);

        entity.Property(x => x.CvContentType)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(x => x.CvFileSize)
            .IsRequired();

        entity.Property(x => x.CoverLetter)
            .HasMaxLength(4000);

        entity.Property(x => x.ReviewNotes)
            .HasMaxLength(2000);

        entity.Property(x => x.Status)
            .IsRequired();

        entity.Property(x => x.SubmittedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        entity.HasOne(x => x.JobPosting)
            .WithMany(x => x.Applications)
            .HasForeignKey(x => x.JobPostingId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.ReviewedBy)
            .WithMany()
            .HasForeignKey(x => x.ReviewedById)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(x => x.CreatedEmployee)
            .WithMany()
            .HasForeignKey(x => x.CreatedEmployeeId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasIndex(x => x.JobPostingId);
        entity.HasIndex(x => x.Status);
        entity.HasIndex(x => new { x.JobPostingId, x.Email });
        entity.HasIndex(x => x.CreatedEmployeeId);
    }
}
