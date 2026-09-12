using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LedgerService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProcessedEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ledger_entries_AccountId",
                table: "ledger_entries");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ledger_entries",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2026, 8, 30, 16, 26, 37, 471, DateTimeKind.Utc).AddTicks(9258));

            migrationBuilder.CreateTable(
                name: "processed_events",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_processed_events", x => x.EventId);
                });

            migrationBuilder.CreateIndex(
                name: "idx_ledger_entries_account_created",
                table: "ledger_entries",
                columns: new[] { "AccountId", "CreatedAt" },
                descending: new[] { false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "processed_events");

            migrationBuilder.DropIndex(
                name: "idx_ledger_entries_account_created",
                table: "ledger_entries");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ledger_entries",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2026, 8, 30, 16, 26, 37, 471, DateTimeKind.Utc).AddTicks(9258),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.CreateIndex(
                name: "IX_ledger_entries_AccountId",
                table: "ledger_entries",
                column: "AccountId");
        }
    }
}
