using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dotamix.Migrations
{
    /// <inheritdoc />
    public partial class updateBracket2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AwayTeamSeed",
                table: "Matches",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HomeTeamSeed",
                table: "Matches",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AwayTeamSeed",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "HomeTeamSeed",
                table: "Matches");
        }
    }
}
