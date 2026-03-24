using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamStrategyAndTasks.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNodeDependencies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "node_dependencies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    blocker_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    blocker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    blocked_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    blocked_id = table.Column<Guid>(type: "uuid", nullable: false),
                    dependency_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_node_dependencies", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_node_dependencies_blocked_type_blocked_id",
                table: "node_dependencies",
                columns: new[] { "blocked_type", "blocked_id" });

            migrationBuilder.CreateIndex(
                name: "ix_node_dependencies_blocker_type_blocker_id",
                table: "node_dependencies",
                columns: new[] { "blocker_type", "blocker_id" });

            migrationBuilder.CreateIndex(
                name: "ix_node_dependencies_blocker_type_blocker_id_blocked_type_bloc",
                table: "node_dependencies",
                columns: new[] { "blocker_type", "blocker_id", "blocked_type", "blocked_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "node_dependencies");
        }
    }
}
