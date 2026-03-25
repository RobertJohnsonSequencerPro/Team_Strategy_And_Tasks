using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamStrategyAndTasks.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddControlPlanModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "control_plans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    process_item = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    part_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    part_description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    revision = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: true),
                    linked_pfmea_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_control_plans", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "control_plan_characteristics",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    control_plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    process_step = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    process_operation = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    characteristic_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    characteristic_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    characteristic_description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    specification_tolerance = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    control_method = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    sampling_size = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    sampling_frequency = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    measurement_technique = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    reaction_plan = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    responsible_person_id = table.Column<Guid>(type: "uuid", nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_control_plan_characteristics", x => x.id);
                    table.ForeignKey(
                        name: "fk_control_plan_characteristics_control_plans_control_plan_id",
                        column: x => x.control_plan_id,
                        principalTable: "control_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "control_plan_revisions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    control_plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    revision_label = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    to_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    comments = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    changed_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    changed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_control_plan_revisions", x => x.id);
                    table.ForeignKey(
                        name: "fk_control_plan_revisions_control_plans_control_plan_id",
                        column: x => x.control_plan_id,
                        principalTable: "control_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_control_plan_characteristics_control_plan_id",
                table: "control_plan_characteristics",
                column: "control_plan_id");

            migrationBuilder.CreateIndex(
                name: "ix_control_plan_characteristics_control_plan_id_sort_order",
                table: "control_plan_characteristics",
                columns: new[] { "control_plan_id", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ix_control_plan_revisions_control_plan_id",
                table: "control_plan_revisions",
                column: "control_plan_id");

            migrationBuilder.CreateIndex(
                name: "ix_control_plan_revisions_control_plan_id_changed_at",
                table: "control_plan_revisions",
                columns: new[] { "control_plan_id", "changed_at" });

            migrationBuilder.CreateIndex(
                name: "ix_control_plans_is_archived_created_at",
                table: "control_plans",
                columns: new[] { "is_archived", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_control_plans_status",
                table: "control_plans",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "control_plan_characteristics");

            migrationBuilder.DropTable(
                name: "control_plan_revisions");

            migrationBuilder.DropTable(
                name: "control_plans");
        }
    }
}
