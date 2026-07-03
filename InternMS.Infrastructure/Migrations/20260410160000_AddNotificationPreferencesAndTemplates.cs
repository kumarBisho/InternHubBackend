using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationPreferencesAndTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create notification_preferences table
            migrationBuilder.CreateTable(
                name: "notification_preferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsTaskAssignedEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsTaskUpdatedEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsTaskCompletedEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsProjectCreatedEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsProjectUpdatedEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsProjectAssignmentEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCommentAddedEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsUserMentionedEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCollaborationInviteEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsDeadlineReminderEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsStatusChangedEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsPriorityChangedEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    EnableEmailNotifications = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    EnableBrowserNotifications = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    EnableSoundNotifications = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    EnableDailyDigest = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    QuietHourStartTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    QuietHourEndTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    IsQuietHourEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_preferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_notification_preferences_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create notification_templates table
            migrationBuilder.CreateTable(
                name: "notification_templates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:Identity", "1, 1"),
                    NotificationType = table.Column<int>(type: "integer", nullable: false),
                    TitleTemplate = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    MessageTemplate = table.Column<string>(type: "text", nullable: false),
                    DescriptionTemplate = table.Column<string>(type: "text", nullable: true),
                    Parameters = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SendInRealTime = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IncludeInEmailDigest = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IncludeInBrowserNotifications = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    PriorityLevel = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    ActionUrlTemplate = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Language = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false, defaultValue: "en"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_templates", x => x.Id);
                });

            // Update notifications table - add new columns
            migrationBuilder.AddColumn<int>(
                name: "PriorityLevel",
                table: "notifications",
                type: "integer",
                nullable: true,
                defaultValue: 3);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "notifications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "notifications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "notifications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedAt",
                table: "notifications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmailSent",
                table: "notifications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailSentAt",
                table: "notifications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "notifications",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BroadcastGroupId",
                table: "notifications",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSystemNotification",
                table: "notifications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Create indexes for notification_preferences
            migrationBuilder.CreateIndex(
                name: "ix_notification_preferences_user_id",
                table: "notification_preferences",
                column: "UserId",
                unique: true);

            // Create indexes for notification_templates
            migrationBuilder.CreateIndex(
                name: "ix_notification_templates_type_language",
                table: "notification_templates",
                columns: new[] { "NotificationType", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_notification_templates_is_active",
                table: "notification_templates",
                column: "IsActive");

            // Create indexes for notifications table enhancements
            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_deleted",
                table: "notifications",
                columns: new[] { "UserId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "ix_notifications_category",
                table: "notifications",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_read",
                table: "notifications",
                columns: new[] { "UserId", "IsRead" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop indexes
            migrationBuilder.DropIndex(
                name: "ix_notifications_user_read",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "ix_notifications_category",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "ix_notifications_user_deleted",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "ix_notification_templates_is_active",
                table: "notification_templates");

            migrationBuilder.DropIndex(
                name: "ix_notification_templates_type_language",
                table: "notification_templates");

            migrationBuilder.DropIndex(
                name: "ix_notification_preferences_user_id",
                table: "notification_preferences");

            // Drop new columns from notifications
            migrationBuilder.DropColumn(
                name: "BroadcastGroupId",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "IsSystemNotification",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "EmailSentAt",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "IsEmailSent",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "PriorityLevel",
                table: "notifications");

            // Drop tables
            migrationBuilder.DropTable(
                name: "notification_templates");

            migrationBuilder.DropTable(
                name: "notification_preferences");
        }
    }
}
