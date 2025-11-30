using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeaMoneyApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addaccountdatainwagelog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "account_id",
                table: "table_wage_log",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_table_wage_log_account_id",
                table: "table_wage_log",
                column: "account_id");

            migrationBuilder.AddForeignKey(
                name: "FK_table_wage_log_table_accounts_account_id",
                table: "table_wage_log",
                column: "account_id",
                principalTable: "table_accounts",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_table_wage_log_table_accounts_account_id",
                table: "table_wage_log");

            migrationBuilder.DropIndex(
                name: "IX_table_wage_log_account_id",
                table: "table_wage_log");

            migrationBuilder.DropColumn(
                name: "account_id",
                table: "table_wage_log");
        }
    }
}
