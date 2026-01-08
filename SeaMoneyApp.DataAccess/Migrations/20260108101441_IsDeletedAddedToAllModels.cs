using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeaMoneyApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class IsDeletedAddedToAllModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "table_wage_log",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "table_salaries",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "table_rub_to_dollar",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "table_positions",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "table_personal_bonuses",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "table_contracts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "table_accounts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "table_wage_log");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "table_salaries");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "table_rub_to_dollar");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "table_positions");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "table_personal_bonuses");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "table_contracts");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "table_accounts");
        }
    }
}
