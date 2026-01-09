using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeaMoneyApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddPositionToContract : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "position_id",
                table: "table_contracts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_table_contracts_position_id",
                table: "table_contracts",
                column: "position_id");

            migrationBuilder.AddForeignKey(
                name: "FK_table_contracts_table_positions_position_id",
                table: "table_contracts",
                column: "position_id",
                principalTable: "table_positions",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_table_contracts_table_positions_position_id",
                table: "table_contracts");

            migrationBuilder.DropIndex(
                name: "IX_table_contracts_position_id",
                table: "table_contracts");

            migrationBuilder.DropColumn(
                name: "position_id",
                table: "table_contracts");
        }
    }
}
