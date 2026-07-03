using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DemoUrl",
                table: "projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentationUrl",
                table: "projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Progress",
                table: "projects",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RepositoryUrl",
                table: "projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TechStack",
                table: "projects",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DemoUrl",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "DocumentationUrl",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "Progress",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "RepositoryUrl",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "TechStack",
                table: "projects");
        }
    }
}
