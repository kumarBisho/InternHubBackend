using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixNotificationPreferencesIdGeneration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Using raw SQL to drop and recreate the notification_preferences table with UUID Id
            migrationBuilder.Sql(@"
                -- Drop existing referential integrity
                DROP TABLE IF EXISTS notification_preferences CASCADE;
                
                -- Recreate table with proper UUID Id column
                CREATE TABLE notification_preferences (
                    ""Id"" uuid NOT NULL DEFAULT gen_random_uuid(),
                    ""UserId"" uuid NOT NULL,
                    ""IsTaskAssignedEnabled"" boolean NOT NULL DEFAULT true,
                    ""IsTaskUpdatedEnabled"" boolean NOT NULL DEFAULT true,
                    ""IsTaskCompletedEnabled"" boolean NOT NULL DEFAULT true,
                    ""IsProjectCreatedEnabled"" boolean NOT NULL DEFAULT true,
                    ""IsProjectUpdatedEnabled"" boolean NOT NULL DEFAULT true,
                    ""IsProjectAssignmentEnabled"" boolean NOT NULL DEFAULT true,
                    ""IsCommentAddedEnabled"" boolean NOT NULL DEFAULT true,
                    ""IsUserMentionedEnabled"" boolean NOT NULL DEFAULT true,
                    ""IsCollaborationInviteEnabled"" boolean NOT NULL DEFAULT true,
                    ""IsDeadlineReminderEnabled"" boolean NOT NULL DEFAULT true,
                    ""IsStatusChangedEnabled"" boolean NOT NULL DEFAULT true,
                    ""IsPriorityChangedEnabled"" boolean NOT NULL DEFAULT true,
                    ""EnableEmailNotifications"" boolean NOT NULL DEFAULT false,
                    ""EnableBrowserNotifications"" boolean NOT NULL DEFAULT true,
                    ""EnableSoundNotifications"" boolean NOT NULL DEFAULT true,
                    ""EnableDailyDigest"" boolean NOT NULL DEFAULT false,
                    ""QuietHourStartTime"" time without time zone NULL,
                    ""QuietHourEndTime"" time without time zone NULL,
                    ""IsQuietHourEnabled"" boolean NOT NULL DEFAULT false,
                    ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT now(),
                    ""UpdatedAt"" timestamp with time zone NOT NULL DEFAULT now(),
                    PRIMARY KEY (""Id""),
                    CONSTRAINT ""FK_notification_preferences_users_UserId"" FOREIGN KEY (""UserId"") REFERENCES users(""Id"") ON DELETE CASCADE
                );
                
                -- Recreate indices
                CREATE INDEX ""IX_notification_preferences_UserId"" ON notification_preferences (""UserId"");
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // This migration is not reversible
            migrationBuilder.Sql("-- Migration is not reversible: Converted NotificationPreferences.Id from int to UUID");
        }
    }
}
