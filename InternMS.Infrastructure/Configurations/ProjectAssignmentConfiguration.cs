using Microsoft.EntityFrameworkCore;
using InternMS.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternMS.Infrastructure.Configurations
{
    public class ProjectAssignmentConfiguration : IEntityTypeConfiguration<ProjectAssignment>
    {
        public void Configure(EntityTypeBuilder<ProjectAssignment> builder)
        {
            builder.ToTable("project_assignments");
            builder.HasKey(pa => pa.Id);

            builder.Property(pa => pa.AssignedAt).HasDefaultValueSql("now()");

            builder.HasOne(pa => pa.Project).WithMany(p => p.Assignments).HasForeignKey(pa => pa.ProjectId);

            builder.HasOne(pa => pa.Intern).WithMany().HasForeignKey(pa => pa.InternId).OnDelete(DeleteBehavior.Restrict);
            
            // MentorId is now nullable, so this relationship is optional
            builder.HasOne(pa => pa.Mentor)
                .WithMany()
                .HasForeignKey(pa => pa.MentorId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            builder.HasIndex(pa => new { pa.ProjectId, pa.InternId }).IsUnique();
        }
    }
}