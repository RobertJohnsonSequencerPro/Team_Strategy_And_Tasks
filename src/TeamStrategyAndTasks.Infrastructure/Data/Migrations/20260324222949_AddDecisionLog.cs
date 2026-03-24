using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamStrategyAndTasks.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDecisionLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "decisions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    context = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    rationale = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    alternatives_considered = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    made_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    made_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    superseded_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_decisions", x => x.id);
                    table.ForeignKey(
                        name: "fk_decisions_decisions_superseded_by_id",
                        column: x => x.superseded_by_id,
                        principalTable: "decisions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "decision_node_links",
                columns: table => new
                {
                    decision_id = table.Column<Guid>(type: "uuid", nullable: false),
                    node_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    node_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_decision_node_links", x => new { x.decision_id, x.node_type, x.node_id });
                    table.ForeignKey(
                        name: "fk_decision_node_links_decisions_decision_id",
                        column: x => x.decision_id,
                        principalTable: "decisions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_decision_node_links_node_type_node_id",
                table: "decision_node_links",
                columns: new[] { "node_type", "node_id" });

            migrationBuilder.CreateIndex(
                name: "ix_decisions_made_at",
                table: "decisions",
                column: "made_at");

            migrationBuilder.CreateIndex(
                name: "ix_decisions_status",
                table: "decisions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_decisions_superseded_by_id",
                table: "decisions",
                column: "superseded_by_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "decision_node_links");

            migrationBuilder.DropTable(
                name: "decisions");
        }
    }
}
