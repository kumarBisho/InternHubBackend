using Microsoft.EntityFrameworkCore;
using InternMS.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternMS.Infrastructure.Configurations
{
    public class ProjectUpdateConfiguration : IEntityTypeConfiguration<ProjectUpdate>
    {
        public void Configure(EntityTypeBuilder<ProjectUpdate> builder)
        {
            builder.ToTable("project_updates");
            builder.HasKey(pu => pu.Id);

            builder.Property(pu => pu.Comment).HasMaxLength(4000);
            builder.Property(pu => pu.CreatedAt).HasDefaultValueSql("now()");
            builder.Property(pu => pu.Status).HasConversion<string>().IsRequired();

            builder.HasOne(pu => pu.Project).WithMany(p => p.Updates).HasForeignKey(pu => pu.ProjectId);
            builder.HasOne(pu => pu.Author).WithMany().HasForeignKey(pu => pu.AuthorId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}