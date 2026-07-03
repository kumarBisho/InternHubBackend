using Microsoft.EntityFrameworkCore;
using InternMS.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternMS.Infrastructure.Configurations
{
    public class ProjectTaskConfiguration : IEntityTypeConfiguration<ProjectTask>
    {
        public void Configure(EntityTypeBuilder<ProjectTask> builder)
        {
            builder.ToTable("project_tasks");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Title).IsRequired().HasMaxLength(255);
            builder.Property(p => p.Status).HasConversion<string>().IsRequired();
            builder.Property(p => p.Priority).HasConversion<string>().IsRequired();
            builder.HasOne(t => t.Project)
              .WithMany(p => p.Tasks)
              .HasForeignKey(t => t.ProjectId)
              .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(t => t.CreatedBy)
              .WithMany(u => u.CreateTasks)
              .HasForeignKey(t => t.CreatedById)
              .OnDelete(DeleteBehavior.Restrict);
        }
    }
}