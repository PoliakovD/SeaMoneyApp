using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeaMoneyApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddedContractsAndUpdateWageLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "contract_id",
                table: "table_wage_log",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "tours_in_rank",
                table: "table_wage_log",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.CreateTable(
                name: "contracts",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    begin_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    end_date = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contracts", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_table_wage_log_contract_id",
                table: "table_wage_log",
                column: "contract_id");

            migrationBuilder.AddForeignKey(
                name: "FK_table_wage_log_contracts_contract_id",
                table: "table_wage_log",
                column: "contract_id",
                principalTable: "contracts",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_table_wage_log_contracts_contract_id",
                table: "table_wage_log");

            migrationBuilder.DropTable(
                name: "contracts");

            migrationBuilder.DropIndex(
                name: "IX_table_wage_log_contract_id",
                table: "table_wage_log");

            migrationBuilder.DropColumn(
                name: "contract_id",
                table: "table_wage_log");

            migrationBuilder.DropColumn(
                name: "tours_in_rank",
                table: "table_wage_log");
        }
    }
}
