using Microsoft.EntityFrameworkCore;
using InternMS.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternMS.Infrastructure.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("notifications");
            builder.HasKey(n => n.Id);

            // Core fields
            builder.Property(n => n.Title).IsRequired().HasMaxLength(255);
            builder.Property(n => n.Message).IsRequired().HasColumnType("text");
            builder.Property(n => n.Description).HasColumnType("text");
            builder.Property(n => n.Type).IsRequired();
            
            // Metadata
            builder.Property(n => n.TriggeredByUserId).IsRequired(false);
            builder.Property(n => n.RelatedEntityId).IsRequired(false);
            builder.Property(n => n.RelatedEntityType).HasMaxLength(50);
            builder.Property(n => n.ActionUrl).HasMaxLength(500);
            
            // Read status
            builder.Property(n => n.IsRead).HasDefaultValue(false);
            builder.Property(n => n.CreatedAt).HasDefaultValueSql("now()");
            builder.Property(n => n.ReadAt).IsRequired(false);
            
            // Enhanced fields
            builder.Property(n => n.PriorityLevel).HasDefaultValue(3);
            builder.Property(n => n.IsDeleted).HasDefaultValue(false);
            builder.Property(n => n.DeletedAt).IsRequired(false);
            builder.Property(n => n.IsArchived).HasDefaultValue(false);
            builder.Property(n => n.ArchivedAt).IsRequired(false);
            builder.Property(n => n.IsEmailSent).HasDefaultValue(false);
            builder.Property(n => n.EmailSentAt).IsRequired(false);
            builder.Property(n => n.Category).HasMaxLength(50);
            builder.Property(n => n.BroadcastGroupId).IsRequired(false);
            builder.Property(n => n.IsSystemNotification).HasDefaultValue(false);
            
            // Foreign key relationship
            builder.HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Indexes for performance
            builder.HasIndex(n => new { n.UserId, n.IsDeleted })
                .HasName("ix_notifications_user_deleted");
            builder.HasIndex(n => n.CreatedAt);
            builder.HasIndex(n => n.Type);
            builder.HasIndex(n => n.Category);
            builder.HasIndex(n => new { n.UserId, n.IsRead })
                .HasName("ix_notifications_user_read");
        }
    }
}