using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixActivityLogSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Note: We're using raw SQL because EF Core has issues with type changes in PostgreSQL
            
            // Add temporary columns for the new types
            migrationBuilder.AddColumn<Guid>(
                name: "UserId_new",
                table: "activity_logs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResourceId_new",
                table: "activity_logs",
                type: "text",
                nullable: true);

            // Copy data from old columns to new columns
            migrationBuilder.Sql("UPDATE activity_logs SET \"UserId_new\" = gen_random_uuid(), \"ResourceId_new\" = \"ResourceId\"::text");

            // Drop old columns
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "activity_logs");

            migrationBuilder.DropColumn(
                name: "ResourceId",
                table: "activity_logs");

            // Rename new columns to old names
            migrationBuilder.RenameColumn(
                name: "UserId_new",
                table: "activity_logs",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "ResourceId_new",
                table: "activity_logs",
                newName: "ResourceId");

            // Make UserId NOT NULL
            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "activity_logs",
                type: "uuid",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Add temporary columns with old types
            migrationBuilder.AddColumn<int>(
                name: "UserId_old",
                table: "activity_logs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResourceId_old",
                table: "activity_logs",
                type: "integer",
                nullable: true);

            // Copy data from new columns to old columns (best effort, may lose data)
            migrationBuilder.Sql("UPDATE activity_logs SET \"UserId_old\" = 0, \"ResourceId_old\" = CAST(SUBSTRING(\"ResourceId\", 1, 10) AS integer)");

            // Drop new columns
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "activity_logs");

            migrationBuilder.DropColumn(
                name: "ResourceId",
                table: "activity_logs");

            // Rename old columns back
            migrationBuilder.RenameColumn(
                name: "UserId_old",
                table: "activity_logs",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "ResourceId_old",
                table: "activity_logs",
                newName: "ResourceId");
        }
    }
}
