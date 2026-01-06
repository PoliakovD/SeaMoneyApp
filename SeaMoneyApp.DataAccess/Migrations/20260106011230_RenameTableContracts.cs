using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeaMoneyApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RenameTableContracts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_table_wage_log_contracts_contract_id",
                table: "table_wage_log");

            migrationBuilder.DropPrimaryKey(
                name: "PK_contracts",
                table: "contracts");

            migrationBuilder.RenameTable(
                name: "contracts",
                newName: "table_contracts");

            migrationBuilder.AddPrimaryKey(
                name: "PK_table_contracts",
                table: "table_contracts",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_table_wage_log_table_contracts_contract_id",
                table: "table_wage_log",
                column: "contract_id",
                principalTable: "table_contracts",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_table_wage_log_table_contracts_contract_id",
                table: "table_wage_log");

            migrationBuilder.DropPrimaryKey(
                name: "PK_table_contracts",
                table: "table_contracts");

            migrationBuilder.RenameTable(
                name: "table_contracts",
                newName: "contracts");

            migrationBuilder.AddPrimaryKey(
                name: "PK_contracts",
                table: "contracts",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_table_wage_log_contracts_contract_id",
                table: "table_wage_log",
                column: "contract_id",
                principalTable: "contracts",
                principalColumn: "id");
        }
    }
}
