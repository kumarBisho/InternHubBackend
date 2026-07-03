using InternMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternMS.Infrastructure.Configurations
{
    public class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
    {
        public void Configure(EntityTypeBuilder<NotificationPreference> builder)
        {
            builder.ToTable("notification_preferences");
            builder.HasKey(np => np.Id);

            // Configure Id to use GUID (no database sequence needed)
            builder.Property(np => np.Id)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("gen_random_uuid()");

            // User relationship
            builder.HasOne(np => np.User)
                .WithMany()
                .HasForeignKey(np => np.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(np => np.UserId).IsRequired();

            // Notification type preferences
            builder.Property(np => np.IsTaskAssignedEnabled).HasDefaultValue(true);
            builder.Property(np => np.IsTaskUpdatedEnabled).HasDefaultValue(true);
            builder.Property(np => np.IsTaskCompletedEnabled).HasDefaultValue(true);
            builder.Property(np => np.IsProjectCreatedEnabled).HasDefaultValue(true);
            builder.Property(np => np.IsProjectUpdatedEnabled).HasDefaultValue(true);
            builder.Property(np => np.IsProjectAssignmentEnabled).HasDefaultValue(true);
            builder.Property(np => np.IsCommentAddedEnabled).HasDefaultValue(true);
            builder.Property(np => np.IsUserMentionedEnabled).HasDefaultValue(true);
            builder.Property(np => np.IsCollaborationInviteEnabled).HasDefaultValue(true);
            builder.Property(np => np.IsDeadlineReminderEnabled).HasDefaultValue(true);
            builder.Property(np => np.IsStatusChangedEnabled).HasDefaultValue(true);
            builder.Property(np => np.IsPriorityChangedEnabled).HasDefaultValue(true);

            // Delivery method preferences
            builder.Property(np => np.EnableEmailNotifications).HasDefaultValue(false);
            builder.Property(np => np.EnableBrowserNotifications).HasDefaultValue(true);
            builder.Property(np => np.EnableSoundNotifications).HasDefaultValue(true);
            builder.Property(np => np.EnableDailyDigest).HasDefaultValue(false);

            // Quiet hours
            builder.Property(np => np.QuietHourStartTime).IsRequired(false);
            builder.Property(np => np.QuietHourEndTime).IsRequired(false);
            builder.Property(np => np.IsQuietHourEnabled).HasDefaultValue(false);

            // Audit columns
            builder.Property(np => np.CreatedAt).HasDefaultValueSql("now()");
            builder.Property(np => np.UpdatedAt).HasDefaultValueSql("now()");

            // Index for quick lookups by user
            builder.HasIndex(np => np.UserId).IsUnique();
        }
    }
}
