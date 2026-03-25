using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamStrategyAndTasks.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRcaModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "rca_cases",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    problem_statement = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    rca_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    linked_capa_case_id = table.Column<Guid>(type: "uuid", nullable: true),
                    linked_finding_id = table.Column<Guid>(type: "uuid", nullable: true),
                    process_area = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    part_family = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    root_cause_summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    initiated_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    approved_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    approved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rca_cases", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "five_why_nodes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    rca_case_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    why_question = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    because_answer = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_root_cause = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_five_why_nodes", x => x.id);
                    table.ForeignKey(
                        name: "fk_five_why_nodes_five_why_nodes_parent_id",
                        column: x => x.parent_id,
                        principalTable: "five_why_nodes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_five_why_nodes_rca_cases_rca_case_id",
                        column: x => x.rca_case_id,
                        principalTable: "rca_cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ishikawa_causes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    rca_case_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    parent_cause_id = table.Column<Guid>(type: "uuid", nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    cause_text = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_root_cause = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ishikawa_causes", x => x.id);
                    table.ForeignKey(
                        name: "fk_ishikawa_causes_ishikawa_causes_parent_cause_id",
                        column: x => x.parent_cause_id,
                        principalTable: "ishikawa_causes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_ishikawa_causes_rca_cases_rca_case_id",
                        column: x => x.rca_case_id,
                        principalTable: "rca_cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rca_case_tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    rca_case_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rca_case_tags", x => x.id);
                    table.ForeignKey(
                        name: "fk_rca_case_tags_rca_cases_rca_case_id",
                        column: x => x.rca_case_id,
                        principalTable: "rca_cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_five_why_nodes_parent_id",
                table: "five_why_nodes",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_five_why_nodes_rca_case_id",
                table: "five_why_nodes",
                column: "rca_case_id");

            migrationBuilder.CreateIndex(
                name: "ix_ishikawa_causes_parent_cause_id",
                table: "ishikawa_causes",
                column: "parent_cause_id");

            migrationBuilder.CreateIndex(
                name: "ix_ishikawa_causes_rca_case_id",
                table: "ishikawa_causes",
                column: "rca_case_id");

            migrationBuilder.CreateIndex(
                name: "ix_ishikawa_causes_rca_case_id_category",
                table: "ishikawa_causes",
                columns: new[] { "rca_case_id", "category" });

            migrationBuilder.CreateIndex(
                name: "ix_rca_case_tags_rca_case_id_tag",
                table: "rca_case_tags",
                columns: new[] { "rca_case_id", "tag" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_rca_case_tags_tag",
                table: "rca_case_tags",
                column: "tag");

            migrationBuilder.CreateIndex(
                name: "ix_rca_cases_is_archived_created_at",
                table: "rca_cases",
                columns: new[] { "is_archived", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_rca_cases_process_area",
                table: "rca_cases",
                column: "process_area");

            migrationBuilder.CreateIndex(
                name: "ix_rca_cases_status",
                table: "rca_cases",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "five_why_nodes");

            migrationBuilder.DropTable(
                name: "ishikawa_causes");

            migrationBuilder.DropTable(
                name: "rca_case_tags");

            migrationBuilder.DropTable(
                name: "rca_cases");
        }
    }
}
