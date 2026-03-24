using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamStrategyAndTasks.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "team_id",
                table: "work_tasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "team_id",
                table: "objectives",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "team_id",
                table: "initiatives",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "team_id",
                table: "business_processes",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "teams",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    mandate = table.Column<string>(type: "text", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_teams", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "team_members",
                columns: table => new
                {
                    team_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    joined_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_team_members", x => new { x.team_id, x.user_id });
                    table.ForeignKey(
                        name: "fk_team_members_teams_team_id",
                        column: x => x.team_id,
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_work_tasks_team_id",
                table: "work_tasks",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "ix_objectives_team_id",
                table: "objectives",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "ix_initiatives_team_id",
                table: "initiatives",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "ix_business_processes_team_id",
                table: "business_processes",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "ix_team_members_user_id",
                table: "team_members",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_teams_is_archived",
                table: "teams",
                column: "is_archived");

            migrationBuilder.CreateIndex(
                name: "ix_teams_name",
                table: "teams",
                column: "name");

            migrationBuilder.AddForeignKey(
                name: "fk_business_processes_teams_team_id",
                table: "business_processes",
                column: "team_id",
                principalTable: "teams",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_initiatives_teams_team_id",
                table: "initiatives",
                column: "team_id",
                principalTable: "teams",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_objectives_teams_team_id",
                table: "objectives",
                column: "team_id",
                principalTable: "teams",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_work_tasks_teams_team_id",
                table: "work_tasks",
                column: "team_id",
                principalTable: "teams",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_business_processes_teams_team_id",
                table: "business_processes");

            migrationBuilder.DropForeignKey(
                name: "fk_initiatives_teams_team_id",
                table: "initiatives");

            migrationBuilder.DropForeignKey(
                name: "fk_objectives_teams_team_id",
                table: "objectives");

            migrationBuilder.DropForeignKey(
                name: "fk_work_tasks_teams_team_id",
                table: "work_tasks");

            migrationBuilder.DropTable(
                name: "team_members");

            migrationBuilder.DropTable(
                name: "teams");

            migrationBuilder.DropIndex(
                name: "ix_work_tasks_team_id",
                table: "work_tasks");

            migrationBuilder.DropIndex(
                name: "ix_objectives_team_id",
                table: "objectives");

            migrationBuilder.DropIndex(
                name: "ix_initiatives_team_id",
                table: "initiatives");

            migrationBuilder.DropIndex(
                name: "ix_business_processes_team_id",
                table: "business_processes");

            migrationBuilder.DropColumn(
                name: "team_id",
                table: "work_tasks");

            migrationBuilder.DropColumn(
                name: "team_id",
                table: "objectives");

            migrationBuilder.DropColumn(
                name: "team_id",
                table: "initiatives");

            migrationBuilder.DropColumn(
                name: "team_id",
                table: "business_processes");
        }
    }
}
