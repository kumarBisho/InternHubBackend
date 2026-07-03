using Microsoft.EntityFrameworkCore;
using InternMS.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternMS.Infrastructure.Configurations
{
    public class ProjectConfiguration : IEntityTypeConfiguration<Project>
    {
        public void Configure(EntityTypeBuilder<Project> builder)
        {
            builder.ToTable("projects");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Title).IsRequired().HasMaxLength(255);
            builder.Property(p => p.Description).HasMaxLength(4000);
            builder.Property(p => p.Status).HasConversion<string>().IsRequired();
            builder.Property(p => p.CreatedAt).HasDefaultValueSql("now()");

            // Relationships
            builder.HasMany(p => p.Assignments).WithOne(a => a.Project).HasForeignKey(a => a.ProjectId);
            builder.HasMany(p => p.Updates).WithOne(u => u.Project).HasForeignKey(u => u.ProjectId);

            builder.HasOne(p => p.CreatedBy).WithMany(u => u.CreateProjects).HasForeignKey(p => p.CreatedById).OnDelete(DeleteBehavior.Restrict);
        }
    }
}