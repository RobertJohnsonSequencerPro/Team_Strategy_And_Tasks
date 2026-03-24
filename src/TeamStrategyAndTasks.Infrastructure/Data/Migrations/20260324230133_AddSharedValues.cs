using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamStrategyAndTasks.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSharedValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "shared_values",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    definition = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shared_values", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_shared_values_is_archived_display_order",
                table: "shared_values",
                columns: new[] { "is_archived", "display_order" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "shared_values");
        }
    }
}
