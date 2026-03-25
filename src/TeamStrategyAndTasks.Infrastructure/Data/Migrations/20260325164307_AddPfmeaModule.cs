using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamStrategyAndTasks.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPfmeaModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pfmea_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    process_item = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    scope = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    revision = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pfmea_records", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pfmea_failure_modes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pfmea_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    process_step = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    process_function = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    failure_description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    potential_effect = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    potential_cause = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    current_controls = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    severity = table.Column<int>(type: "integer", nullable: false),
                    occurrence = table.Column<int>(type: "integer", nullable: false),
                    detection = table.Column<int>(type: "integer", nullable: false),
                    severity_after = table.Column<int>(type: "integer", nullable: true),
                    occurrence_after = table.Column<int>(type: "integer", nullable: true),
                    detection_after = table.Column<int>(type: "integer", nullable: true),
                    assigned_to_id = table.Column<Guid>(type: "uuid", nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pfmea_failure_modes", x => x.id);
                    table.ForeignKey(
                        name: "fk_pfmea_failure_modes_pfmea_records_pfmea_id",
                        column: x => x.pfmea_id,
                        principalTable: "pfmea_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pfmea_actions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    failure_mode_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    assigned_to_id = table.Column<Guid>(type: "uuid", nullable: true),
                    target_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completion_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    outcome_notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pfmea_actions", x => x.id);
                    table.ForeignKey(
                        name: "fk_pfmea_actions_pfmea_failure_modes_failure_mode_id",
                        column: x => x.failure_mode_id,
                        principalTable: "pfmea_failure_modes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_pfmea_actions_failure_mode_id",
                table: "pfmea_actions",
                column: "failure_mode_id");

            migrationBuilder.CreateIndex(
                name: "ix_pfmea_actions_status_target_date",
                table: "pfmea_actions",
                columns: new[] { "status", "target_date" });

            migrationBuilder.CreateIndex(
                name: "ix_pfmea_failure_modes_pfmea_id",
                table: "pfmea_failure_modes",
                column: "pfmea_id");

            migrationBuilder.CreateIndex(
                name: "ix_pfmea_failure_modes_pfmea_id_sort_order",
                table: "pfmea_failure_modes",
                columns: new[] { "pfmea_id", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ix_pfmea_records_is_archived_created_at",
                table: "pfmea_records",
                columns: new[] { "is_archived", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pfmea_actions");

            migrationBuilder.DropTable(
                name: "pfmea_failure_modes");

            migrationBuilder.DropTable(
                name: "pfmea_records");
        }
    }
}
