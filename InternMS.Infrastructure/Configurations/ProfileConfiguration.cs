using Microsoft.EntityFrameworkCore;
using InternMS.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternMS.Infrastructure.Configurations
{
    public class ProfileConfiguration : IEntityTypeConfiguration<UserProfile>
    {
        public void Configure(EntityTypeBuilder<UserProfile> builder)
        {
            builder.ToTable("profiles");
            builder.HasKey(p => p.UserId);

            builder.Property(p => p.Phone).HasMaxLength(50);
            builder.Property(p => p.Department).HasMaxLength(100);
            builder.Property(p => p.Position).HasMaxLength(100);
            builder.Property(p => p.Bio).HasMaxLength(1000);
        }
    }
}