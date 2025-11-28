using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeaMoneyApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "table_positions",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_table_positions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "table_rub_to_dollar",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    value = table.Column<decimal>(type: "TEXT", nullable: false),
                    date = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_table_rub_to_dollar", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "table_accounts",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    login = table.Column<string>(type: "TEXT", nullable: false),
                    password = table.Column<string>(type: "TEXT", nullable: false),
                    position_id = table.Column<int>(type: "INTEGER", nullable: true),
                    tours_in_rank = table.Column<short>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_table_accounts", x => x.id);
                    table.ForeignKey(
                        name: "FK_table_accounts_table_positions_position_id",
                        column: x => x.position_id,
                        principalTable: "table_positions",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "table_personal_bonuses",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    year = table.Column<short>(type: "INTEGER", nullable: false),
                    position_id = table.Column<int>(type: "INTEGER", nullable: true),
                    personal_bonus = table.Column<decimal>(type: "TEXT", nullable: false),
                    tours_in_rank = table.Column<short>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_table_personal_bonuses", x => x.id);
                    table.ForeignKey(
                        name: "FK_table_personal_bonuses_table_positions_position_id",
                        column: x => x.position_id,
                        principalTable: "table_positions",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "table_salaries",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    year = table.Column<short>(type: "INTEGER", nullable: false),
                    position_id = table.Column<int>(type: "INTEGER", nullable: true),
                    basic_wage = table.Column<decimal>(type: "TEXT", nullable: false),
                    crew_overtime = table.Column<decimal>(type: "TEXT", nullable: false),
                    fidelity_bonus = table.Column<decimal>(type: "TEXT", nullable: false),
                    vacation = table.Column<decimal>(type: "TEXT", nullable: false),
                    company_bonus = table.Column<decimal>(type: "TEXT", nullable: false),
                    performance_bonus = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_table_salaries", x => x.id);
                    table.ForeignKey(
                        name: "FK_table_salaries_table_positions_position_id",
                        column: x => x.position_id,
                        principalTable: "table_positions",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "table_wage_log",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    amount_in_rub = table.Column<decimal>(type: "TEXT", nullable: false),
                    change_rub_to_dollar_id = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_table_wage_log", x => x.id);
                    table.ForeignKey(
                        name: "FK_table_wage_log_table_rub_to_dollar_change_rub_to_dollar_id",
                        column: x => x.change_rub_to_dollar_id,
                        principalTable: "table_rub_to_dollar",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_table_accounts_position_id",
                table: "table_accounts",
                column: "position_id");

            migrationBuilder.CreateIndex(
                name: "IX_table_personal_bonuses_position_id",
                table: "table_personal_bonuses",
                column: "position_id");

            migrationBuilder.CreateIndex(
                name: "IX_table_salaries_position_id",
                table: "table_salaries",
                column: "position_id");

            migrationBuilder.CreateIndex(
                name: "IX_table_wage_log_change_rub_to_dollar_id",
                table: "table_wage_log",
                column: "change_rub_to_dollar_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "table_accounts");

            migrationBuilder.DropTable(
                name: "table_personal_bonuses");

            migrationBuilder.DropTable(
                name: "table_salaries");

            migrationBuilder.DropTable(
                name: "table_wage_log");

            migrationBuilder.DropTable(
                name: "table_positions");

            migrationBuilder.DropTable(
                name: "table_rub_to_dollar");
        }
    }
}
