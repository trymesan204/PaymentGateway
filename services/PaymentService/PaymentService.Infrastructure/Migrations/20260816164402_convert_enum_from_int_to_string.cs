using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class convert_enum_from_int_to_string : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "payments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2026, 8, 16, 16, 44, 2, 447, DateTimeKind.Utc).AddTicks(2806),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2026, 8, 16, 16, 39, 0, 838, DateTimeKind.Utc).AddTicks(2075));

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "payments",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "payments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2026, 8, 16, 16, 44, 2, 447, DateTimeKind.Utc).AddTicks(2479),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2026, 8, 16, 16, 39, 0, 838, DateTimeKind.Utc).AddTicks(1696));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "payments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2026, 8, 16, 16, 39, 0, 838, DateTimeKind.Utc).AddTicks(2075),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2026, 8, 16, 16, 44, 2, 447, DateTimeKind.Utc).AddTicks(2806));

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "payments",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "payments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2026, 8, 16, 16, 39, 0, 838, DateTimeKind.Utc).AddTicks(1696),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2026, 8, 16, 16, 44, 2, 447, DateTimeKind.Utc).AddTicks(2479));
        }
    }
}
