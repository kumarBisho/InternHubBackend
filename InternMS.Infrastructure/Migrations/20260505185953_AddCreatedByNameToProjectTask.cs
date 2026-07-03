using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByNameToProjectTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "project_tasks",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "project_tasks");
        }
    }
}