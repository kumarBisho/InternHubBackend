using InternMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternMS.Infrastructure.Configurations
{
    public class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
    {
        public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
        {
            builder.ToTable("notification_templates");
            builder.HasKey(nt => nt.Id);

            // Template content
            builder.Property(nt => nt.NotificationType).IsRequired();
            builder.Property(nt => nt.TitleTemplate).IsRequired().HasMaxLength(500);
            builder.Property(nt => nt.MessageTemplate).IsRequired().HasColumnType("text");
            builder.Property(nt => nt.DescriptionTemplate).HasColumnType("text");
            builder.Property(nt => nt.Parameters).HasMaxLength(1000);
            builder.Property(nt => nt.ActionUrlTemplate).HasMaxLength(500);

            // Delivery preferences
            builder.Property(nt => nt.SendInRealTime).HasDefaultValue(true);
            builder.Property(nt => nt.IncludeInEmailDigest).HasDefaultValue(true);
            builder.Property(nt => nt.IncludeInBrowserNotifications).HasDefaultValue(true);

            // Priority and localization
            builder.Property(nt => nt.PriorityLevel).HasDefaultValue(3);
            builder.Property(nt => nt.Language).IsRequired().HasDefaultValue("en").HasMaxLength(5);
            builder.Property(nt => nt.IsActive).HasDefaultValue(true);
            builder.Property(nt => nt.Notes).HasColumnType("text");

            // Audit columns
            builder.Property(nt => nt.CreatedAt).HasDefaultValueSql("now()");
            builder.Property(nt => nt.UpdatedAt).HasDefaultValueSql("now()");

            // Index for quick lookups
            builder.HasIndex(nt => new { nt.NotificationType, nt.Language })
                .IsUnique()
                .HasName("ix_notification_templates_type_language");

            builder.HasIndex(nt => nt.IsActive);
        }
    }
}
