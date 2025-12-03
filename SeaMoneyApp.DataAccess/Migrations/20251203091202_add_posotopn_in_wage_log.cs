using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeaMoneyApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class add_posotopn_in_wage_log : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "position_id",
                table: "table_wage_log",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_table_wage_log_position_id",
                table: "table_wage_log",
                column: "position_id");

            migrationBuilder.AddForeignKey(
                name: "FK_table_wage_log_table_positions_position_id",
                table: "table_wage_log",
                column: "position_id",
                principalTable: "table_positions",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_table_wage_log_table_positions_position_id",
                table: "table_wage_log");

            migrationBuilder.DropIndex(
                name: "IX_table_wage_log_position_id",
                table: "table_wage_log");

            migrationBuilder.DropColumn(
                name: "position_id",
                table: "table_wage_log");
        }
    }
}
