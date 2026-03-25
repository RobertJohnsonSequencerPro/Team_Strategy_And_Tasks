using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamStrategyAndTasks.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditCapaModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audits",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    audit_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    scope = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    lead_auditor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    scheduled_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    start_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completed_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audits", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "capa_cases",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    case_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    capa_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    problem_statement = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    containment_actions = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    root_cause_analysis = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    proposed_correction = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: true),
                    target_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completed_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false),
                    linked_finding_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_capa_cases", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "audit_findings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    audit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    finding_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    clause_reference = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    assigned_to_id = table.Column<Guid>(type: "uuid", nullable: true),
                    due_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    containment_notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    root_cause_notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    corrective_action_notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    linked_capa_case_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_findings", x => x.id);
                    table.ForeignKey(
                        name: "fk_audit_findings_audits_audit_id",
                        column: x => x.audit_id,
                        principalTable: "audits",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "capa_actions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    capa_case_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    assigned_to_id = table.Column<Guid>(type: "uuid", nullable: true),
                    target_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completion_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_capa_actions", x => x.id);
                    table.ForeignKey(
                        name: "fk_capa_actions_capa_cases_capa_case_id",
                        column: x => x.capa_case_id,
                        principalTable: "capa_cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "effectiveness_checks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    capa_case_id = table.Column<Guid>(type: "uuid", nullable: false),
                    check_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    checked_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    verdict = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    next_check_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_effectiveness_checks", x => x.id);
                    table.ForeignKey(
                        name: "fk_effectiveness_checks_capa_cases_capa_case_id",
                        column: x => x.capa_case_id,
                        principalTable: "capa_cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_audit_findings_audit_id",
                table: "audit_findings",
                column: "audit_id");

            migrationBuilder.CreateIndex(
                name: "ix_audit_findings_audit_id_status",
                table: "audit_findings",
                columns: new[] { "audit_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_findings_status",
                table: "audit_findings",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_audits_is_archived_created_at",
                table: "audits",
                columns: new[] { "is_archived", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_audits_status",
                table: "audits",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_capa_actions_capa_case_id",
                table: "capa_actions",
                column: "capa_case_id");

            migrationBuilder.CreateIndex(
                name: "ix_capa_actions_status",
                table: "capa_actions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_capa_cases_case_number",
                table: "capa_cases",
                column: "case_number");

            migrationBuilder.CreateIndex(
                name: "ix_capa_cases_is_archived_created_at",
                table: "capa_cases",
                columns: new[] { "is_archived", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_capa_cases_status",
                table: "capa_cases",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_effectiveness_checks_capa_case_id",
                table: "effectiveness_checks",
                column: "capa_case_id");

            migrationBuilder.CreateIndex(
                name: "ix_effectiveness_checks_capa_case_id_check_date",
                table: "effectiveness_checks",
                columns: new[] { "capa_case_id", "check_date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_findings");

            migrationBuilder.DropTable(
                name: "capa_actions");

            migrationBuilder.DropTable(
                name: "effectiveness_checks");

            migrationBuilder.DropTable(
                name: "audits");

            migrationBuilder.DropTable(
                name: "capa_cases");
        }
    }
}
