using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace InternMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectTaskAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_project_tasks_users_UserId",
                table: "project_tasks");

            migrationBuilder.DropIndex(
                name: "IX_project_tasks_UserId",
                table: "project_tasks");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "project_tasks");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "project_tasks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "project_task_assignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    InternId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_task_assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_project_task_assignments_project_tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "project_tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_project_task_assignments_users_InternId",
                        column: x => x.InternId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_project_tasks_CreatedById",
                table: "project_tasks",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_project_task_assignments_InternId",
                table: "project_task_assignments",
                column: "InternId");

            migrationBuilder.CreateIndex(
                name: "IX_project_task_assignments_TaskId_InternId",
                table: "project_task_assignments",
                columns: new[] { "TaskId", "InternId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_project_tasks_users_CreatedById",
                table: "project_tasks",
                column: "CreatedById",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_project_tasks_users_CreatedById",
                table: "project_tasks");

            migrationBuilder.DropTable(
                name: "project_task_assignments");

            migrationBuilder.DropIndex(
                name: "IX_project_tasks_CreatedById",
                table: "project_tasks");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "project_tasks");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "project_tasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_project_tasks_UserId",
                table: "project_tasks",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_project_tasks_users_UserId",
                table: "project_tasks",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id");
        }
    }
}
