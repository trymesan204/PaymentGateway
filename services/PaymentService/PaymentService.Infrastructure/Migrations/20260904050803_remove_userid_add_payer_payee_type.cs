using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class remove_userid_add_payer_payee_type : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "payments");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "payments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2026, 9, 4, 5, 8, 3, 117, DateTimeKind.Utc).AddTicks(4779),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2026, 9, 4, 4, 38, 9, 464, DateTimeKind.Utc).AddTicks(8714));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "payments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2026, 9, 4, 5, 8, 3, 117, DateTimeKind.Utc).AddTicks(4330),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2026, 9, 4, 4, 38, 9, 464, DateTimeKind.Utc).AddTicks(8263));

            migrationBuilder.AddColumn<Guid>(
                name: "PayerId",
                table: "payments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PayeeId",
                table: "payments",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "payments",
                type: "text",
                nullable: false,
                defaultValue: nameof(PaymentService.Domain.Enums.PaymentType.TopUp));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PayerId",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "PayeeId",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "payments");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "payments",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "payments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2026, 9, 4, 4, 38, 9, 464, DateTimeKind.Utc).AddTicks(8714),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2026, 9, 4, 5, 8, 3, 117, DateTimeKind.Utc).AddTicks(4779));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "payments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2026, 9, 4, 4, 38, 9, 464, DateTimeKind.Utc).AddTicks(8263),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2026, 9, 4, 5, 8, 3, 117, DateTimeKind.Utc).AddTicks(4330));
        }
    }
}
