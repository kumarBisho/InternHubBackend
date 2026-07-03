using Microsoft.EntityFrameworkCore;
using InternMS.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternMS.Infrastructure.Configurations
{
    public class ProjectTaskAssignmentConfiguration : IEntityTypeConfiguration<ProjectTaskAssignment>
    {
        public void Configure(EntityTypeBuilder<ProjectTaskAssignment> builder)
        {
            builder.ToTable("project_task_assignments");
            builder.HasKey(pta => pta.Id);

            builder.Property(pta => pta.AssignedAt).HasDefaultValueSql("now()");

            builder.HasOne(pta => pta.Task)
                .WithMany(t => t.Assignments)
                .HasForeignKey(pta => pta.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pta => pta.Intern)
                .WithMany()
                .HasForeignKey(pta => pta.InternId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(pta => new { pta.TaskId, pta.InternId }).IsUnique();
        }
    }
}
