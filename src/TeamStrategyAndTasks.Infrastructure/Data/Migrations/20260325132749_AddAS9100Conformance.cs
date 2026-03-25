using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamStrategyAndTasks.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAS9100Conformance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "quality_clauses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    clause_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    conformance_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    assigned_to_id = table.Column<Guid>(type: "uuid", nullable: true),
                    review_due_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    assessment_notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    last_reviewed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_reviewed_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_quality_clauses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "clause_evidence_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    clause_id = table.Column<Guid>(type: "uuid", nullable: false),
                    evidence_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    added_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_clause_evidence_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_clause_evidence_items_quality_clauses_clause_id",
                        column: x => x.clause_id,
                        principalTable: "quality_clauses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "clause_review_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    clause_id = table.Column<Guid>(type: "uuid", nullable: false),
                    previous_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    new_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    reviewed_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reviewed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_clause_review_events", x => x.id);
                    table.ForeignKey(
                        name: "fk_clause_review_events_quality_clauses_clause_id",
                        column: x => x.clause_id,
                        principalTable: "quality_clauses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_clause_evidence_items_clause_id",
                table: "clause_evidence_items",
                column: "clause_id");

            migrationBuilder.CreateIndex(
                name: "ix_clause_review_events_clause_id",
                table: "clause_review_events",
                column: "clause_id");

            migrationBuilder.CreateIndex(
                name: "ix_clause_review_events_reviewed_at",
                table: "clause_review_events",
                column: "reviewed_at");

            migrationBuilder.CreateIndex(
                name: "ix_quality_clauses_clause_number",
                table: "quality_clauses",
                column: "clause_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_quality_clauses_is_active_sort_order",
                table: "quality_clauses",
                columns: new[] { "is_active", "sort_order" });

            // ── AS9100D Clause Seed Data ──────────────────────────────────────────
            var seedTime = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var clauses = new[]
            {
                // Section 4 — Context of the Organization
                (new Guid("10000000-0000-0000-0000-000000000001"), "4.1",  "Understanding the Organization and Its Context",                           1,
                    "Determine internal and external issues relevant to the QMS purpose and strategic direction."),
                (new Guid("10000000-0000-0000-0000-000000000002"), "4.2",  "Understanding the Needs and Expectations of Interested Parties",           2,
                    "Identify interested parties and their relevant requirements for the QMS."),
                (new Guid("10000000-0000-0000-0000-000000000003"), "4.3",  "Determining the Scope of the Quality Management System",                  3,
                    "Define the boundaries and applicability of the QMS. Include product/service scope and applicable standards."),
                (new Guid("10000000-0000-0000-0000-000000000004"), "4.4",  "Quality Management System and Its Processes",                             4,
                    "Establish, implement, maintain and continually improve the QMS and its processes, sequences and interactions."),
                // Section 5 — Leadership
                (new Guid("10000000-0000-0000-0000-000000000005"), "5.1",  "Leadership and Commitment",                                               5,
                    "Top management demonstrates leadership by taking accountability, promoting process approach, and supporting the QMS."),
                (new Guid("10000000-0000-0000-0000-000000000006"), "5.2",  "Policy",                                                                  6,
                    "Establish, implement and communicate a quality policy appropriate to the organization's context."),
                (new Guid("10000000-0000-0000-0000-000000000007"), "5.3",  "Organizational Roles, Responsibilities and Authorities",                  7,
                    "Assign and communicate roles, responsibilities and authorities for QMS-relevant functions."),
                // Section 6 — Planning
                (new Guid("10000000-0000-0000-0000-000000000008"), "6.1",  "Actions to Address Risks and Opportunities",                              8,
                    "Determine risks and opportunities to ensure the QMS achieves intended results. Plan and implement actions."),
                (new Guid("10000000-0000-0000-0000-000000000009"), "6.2",  "Quality Objectives and Planning to Achieve Them",                         9,
                    "Establish measurable quality objectives at relevant functions/levels. Plans shall include who, what, when and how."),
                (new Guid("10000000-0000-0000-0000-000000000010"), "6.3",  "Planning of Changes",                                                    10,
                    "Changes to the QMS shall be carried out in a planned manner considering purpose, integrity, resources and responsibilities."),
                // Section 7 — Support
                (new Guid("10000000-0000-0000-0000-000000000011"), "7.1",  "Resources",                                                              11,
                    "Determine and provide resources for the establishment, implementation, maintenance and improvement of the QMS."),
                (new Guid("10000000-0000-0000-0000-000000000012"), "7.2",  "Competence",                                                             12,
                    "Determine competence of persons affecting QMS performance. Ensure education, training or experience. Retain documented information."),
                (new Guid("10000000-0000-0000-0000-000000000013"), "7.3",  "Awareness",                                                              13,
                    "Persons must be aware of the quality policy, objectives, QMS contributions and nonconformity implications."),
                (new Guid("10000000-0000-0000-0000-000000000014"), "7.4",  "Communication",                                                          14,
                    "Determine internal and external communications relevant to the QMS: what, when, with whom, how, and who communicates."),
                (new Guid("10000000-0000-0000-0000-000000000015"), "7.5",  "Documented Information",                                                 15,
                    "Create, update and control documented information required by the standard and determined as necessary for effectiveness."),
                // Section 8 — Operation
                (new Guid("10000000-0000-0000-0000-000000000016"), "8.1",  "Operational Planning and Control",                                       16,
                    "Plan, implement, control, maintain and retain documented information for processes needed to meet requirements. Includes operational risk management (AS9100D)."),
                (new Guid("10000000-0000-0000-0000-000000000017"), "8.2",  "Requirements for Products and Services",                                 17,
                    "Determine, review and communicate customer requirements, statutory/regulatory requirements, and contract review outcomes."),
                (new Guid("10000000-0000-0000-0000-000000000018"), "8.3",  "Design and Development of Products and Services",                        18,
                    "Establish, implement and maintain a design and development process, including planning, inputs, controls, outputs and changes."),
                (new Guid("10000000-0000-0000-0000-000000000019"), "8.4",  "Control of Externally Provided Processes, Products and Services",        19,
                    "Ensure externally provided processes, products and services conform to requirements. Includes supplier evaluation and monitoring."),
                (new Guid("10000000-0000-0000-0000-000000000020"), "8.5",  "Production and Service Provision",                                       20,
                    "Implement production and service provision under controlled conditions. Includes traceability, customer property, and preservation."),
                (new Guid("10000000-0000-0000-0000-000000000021"), "8.6",  "Release of Products and Services",                                       21,
                    "Product/service release shall not proceed until planned arrangements have been satisfactorily completed. Retain evidence of conformity."),
                (new Guid("10000000-0000-0000-0000-000000000022"), "8.7",  "Control of Nonconforming Outputs",                                       22,
                    "Identify and control outputs that do not conform to requirements. Take appropriate action. Retain documented information."),
                // Section 9 — Performance Evaluation
                (new Guid("10000000-0000-0000-0000-000000000023"), "9.1",  "Monitoring, Measurement, Analysis and Evaluation",                       23,
                    "Determine what, when and how to monitor and measure. Evaluate QMS performance and effectiveness. Retain documented information."),
                (new Guid("10000000-0000-0000-0000-000000000024"), "9.2",  "Internal Audit",                                                         24,
                    "Plan, establish and conduct internal audits at planned intervals. Address nonconformities. Report audit results to management."),
                (new Guid("10000000-0000-0000-0000-000000000025"), "9.3",  "Management Review",                                                      25,
                    "Top management reviews QMS at planned intervals for continued suitability, adequacy, effectiveness and alignment with strategic direction."),
                // Section 10 — Improvement
                (new Guid("10000000-0000-0000-0000-000000000026"), "10.1", "General",                                                                26,
                    "Determine and select opportunities for improvement and implement actions to meet customer requirements and enhance satisfaction."),
                (new Guid("10000000-0000-0000-0000-000000000027"), "10.2", "Nonconformity and Corrective Action",                                    27,
                    "React to nonconformities, take corrective actions, and review effectiveness. Retain documented information of results."),
                (new Guid("10000000-0000-0000-0000-000000000028"), "10.3", "Continual Improvement",                                                  28,
                    "Continually improve the suitability, adequacy and effectiveness of the QMS using quality policy, objectives and audit results."),
            };

            foreach (var (id, clauseNumber, title, sortOrder, description) in clauses)
            {
                migrationBuilder.InsertData(
                    table: "quality_clauses",
                    columns: new[] { "id", "clause_number", "title", "description", "sort_order", "is_active",
                                     "conformance_status", "assigned_to_id", "review_due_date",
                                     "assessment_notes", "last_reviewed_at", "last_reviewed_by_id",
                                     "created_at", "updated_at" },
                    values: new object[] { id, clauseNumber, title, description, sortOrder, true,
                                          "NotAssessed", null, null, null, null, null,
                                          seedTime, seedTime });
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "clause_evidence_items");

            migrationBuilder.DropTable(
                name: "clause_review_events");

            migrationBuilder.DropTable(
                name: "quality_clauses");
        }
    }
}
