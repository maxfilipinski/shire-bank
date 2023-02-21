using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShireBank.Repository.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIsClosedFromBankAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsClosed",
                table: "Accounts");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Transactions",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2023, 2, 21, 21, 53, 57, 312, DateTimeKind.Utc).AddTicks(2249),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2023, 2, 20, 8, 0, 23, 446, DateTimeKind.Utc).AddTicks(9656));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Transactions",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2023, 2, 20, 8, 0, 23, 446, DateTimeKind.Utc).AddTicks(9656),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2023, 2, 21, 21, 53, 57, 312, DateTimeKind.Utc).AddTicks(2249));

            migrationBuilder.AddColumn<bool>(
                name: "IsClosed",
                table: "Accounts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
