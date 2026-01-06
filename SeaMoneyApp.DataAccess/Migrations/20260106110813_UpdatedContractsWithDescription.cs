using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeaMoneyApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedContractsWithDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "end_date",
                table: "table_contracts",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.AddColumn<int>(
                name: "account_id",
                table: "table_contracts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "contract_description",
                table: "table_contracts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "vessel_name",
                table: "table_contracts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_table_contracts_account_id",
                table: "table_contracts",
                column: "account_id");

            migrationBuilder.AddForeignKey(
                name: "FK_table_contracts_table_accounts_account_id",
                table: "table_contracts",
                column: "account_id",
                principalTable: "table_accounts",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_table_contracts_table_accounts_account_id",
                table: "table_contracts");

            migrationBuilder.DropIndex(
                name: "IX_table_contracts_account_id",
                table: "table_contracts");

            migrationBuilder.DropColumn(
                name: "account_id",
                table: "table_contracts");

            migrationBuilder.DropColumn(
                name: "contract_description",
                table: "table_contracts");

            migrationBuilder.DropColumn(
                name: "vessel_name",
                table: "table_contracts");

            migrationBuilder.AlterColumn<DateTime>(
                name: "end_date",
                table: "table_contracts",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
