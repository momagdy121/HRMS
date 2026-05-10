using HRSystem.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRSystem.Data.Configurations;

public class EmployeeTaskConfiguration : IEntityTypeConfiguration<EmployeeTask>
{
    public void Configure(EntityTypeBuilder<EmployeeTask> entity)
    {
        entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Description).HasMaxLength(1000);
        entity.Property(x => x.IsDeleted).HasDefaultValue(false);
        entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        entity.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(x => x.AssignedById)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(x => x.AssignedToId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasData(
            new EmployeeTask
            {
                Id = 1,
                Title = "Review API documentation",
                Description = "Update internal wiki for onboarding.",
                AssignedById = 2,
                AssignedToId = 4,
                Status = global::HRSystem.Common.Enums.TaskStatus.Pending,
                DueDate = new DateOnly(2025, 11, 30),
                IsDeleted = false,
                CreatedAt = SeedValues.TaskCreatedAt1,
                UpdatedAt = null
            },
            new EmployeeTask
            {
                Id = 2,
                Title = "Prepare sprint demo",
                Description = "Record short walkthrough of dashboard features.",
                AssignedById = 2,
                AssignedToId = 6,
                Status = global::HRSystem.Common.Enums.TaskStatus.InProgress,
                DueDate = new DateOnly(2025, 10, 15),
                IsDeleted = false,
                CreatedAt = SeedValues.TaskCreatedAt2,
                UpdatedAt = SeedValues.TaskUpdatedAt1
            },
            new EmployeeTask
            {
                Id = 3,
                Title = "Update HR policy checklist",
                AssignedById = 3,
                AssignedToId = 5,
                Status = global::HRSystem.Common.Enums.TaskStatus.Completed,
                DueDate = new DateOnly(2025, 9, 1),
                IsDeleted = false,
                CreatedAt = SeedValues.TaskCreatedAt1,
                UpdatedAt = SeedValues.TaskUpdatedAt1
            },
            new EmployeeTask
            {
                Id = 4,
                Title = "Schedule 1:1 meetings",
                AssignedById = 3,
                AssignedToId = 7,
                Status = global::HRSystem.Common.Enums.TaskStatus.Pending,
                DueDate = null,
                IsDeleted = false,
                CreatedAt = SeedValues.TaskCreatedAt2,
                UpdatedAt = null
            });
    }
}
