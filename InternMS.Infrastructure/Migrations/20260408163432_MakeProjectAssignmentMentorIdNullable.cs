using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeProjectAssignmentMentorIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "MentorId",
                table: "project_assignments",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "ActionUrl",
                table: "notifications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "notifications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReadAt",
                table: "notifications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RelatedEntityId",
                table: "notifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RelatedEntityType",
                table: "notifications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TriggeredByUserId",
                table: "notifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "notifications",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActionUrl",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "ReadAt",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "RelatedEntityId",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "RelatedEntityType",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "TriggeredByUserId",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "notifications");

            migrationBuilder.AlterColumn<Guid>(
                name: "MentorId",
                table: "project_assignments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
