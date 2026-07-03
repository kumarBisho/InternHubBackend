using Microsoft.EntityFrameworkCore;
using InternMS.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternMS.Infrastructure.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Email).IsRequired().HasMaxLength(255);
            builder.HasIndex(u => u.Email).IsUnique();
            builder.Property(u => u.PasswordHash).IsRequired();
            builder.Property(u => u.FirstName).HasMaxLength(100);
            builder.Property(u => u.LastName).HasMaxLength(100);
            builder.Property(u => u.EmailConfirmed).HasDefaultValue(false);
            builder.Property(u => u.AdminApproved).HasDefaultValue(false);
            builder.Property(u => u.EmailConfirmationToken).HasMaxLength(255);
            builder.Property(u => u.EmailConfirmationTokenExpires);
            builder.Property(u => u.IsActive).HasDefaultValue(false);
            builder.Property(u => u.CreatedAt).HasDefaultValueSql("now()");

            // Relationships 
            builder.HasMany(u => u.UserRoles).WithOne(ur => ur.User).HasForeignKey(ur => ur.UserId);
            builder.HasOne(u => u.Profile).WithOne(p => p.User).HasForeignKey<UserProfile>(p => p.UserId);
            builder.HasMany(u => u.CreateProjects).WithOne(p => p.CreatedBy).HasForeignKey(p => p.CreatedById);
            builder.HasOne(u => u.RefreshToken).WithOne(rt => rt.User).HasForeignKey<RefreshToken>(rt => rt.UserId);
        }
    }
}