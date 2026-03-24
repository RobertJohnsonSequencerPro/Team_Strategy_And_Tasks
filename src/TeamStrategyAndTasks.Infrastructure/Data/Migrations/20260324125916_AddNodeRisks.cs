using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamStrategyAndTasks.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNodeRisks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "node_risks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    node_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    node_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    probability = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    impact = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    mitigation_plan = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    raised_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    resolved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_node_risks", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_node_risks_node_type_node_id",
                table: "node_risks",
                columns: new[] { "node_type", "node_id" });

            migrationBuilder.CreateIndex(
                name: "ix_node_risks_status_probability_impact",
                table: "node_risks",
                columns: new[] { "status", "probability", "impact" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "node_risks");
        }
    }
}
