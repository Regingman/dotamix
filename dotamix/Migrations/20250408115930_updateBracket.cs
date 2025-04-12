using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dotamix.Migrations
{
    /// <inheritdoc />
    public partial class updateBracket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Tournaments");

            migrationBuilder.RenameColumn(
                name: "IsCompleted",
                table: "Tournaments",
                newName: "IsGrandFinalNeeded");

            migrationBuilder.AddColumn<int>(
                name: "CurrentBracket",
                table: "Tournaments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrentRound",
                table: "Tournaments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfTeams",
                table: "Tournaments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Tournaments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ScheduledTime",
                table: "Matches",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AddColumn<int>(
                name: "BracketType",
                table: "Matches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsGrandFinal",
                table: "Matches",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSecondGrandFinal",
                table: "Matches",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LoserNextMatchId",
                table: "Matches",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LoserNextMatchPosition",
                table: "Matches",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WinnerNextMatchId",
                table: "Matches",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WinnerNextMatchPosition",
                table: "Matches",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Matches_LoserNextMatchId",
                table: "Matches",
                column: "LoserNextMatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_WinnerNextMatchId",
                table: "Matches",
                column: "WinnerNextMatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Matches_LoserNextMatchId",
                table: "Matches",
                column: "LoserNextMatchId",
                principalTable: "Matches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Matches_WinnerNextMatchId",
                table: "Matches",
                column: "WinnerNextMatchId",
                principalTable: "Matches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Matches_LoserNextMatchId",
                table: "Matches");

            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Matches_WinnerNextMatchId",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_Matches_LoserNextMatchId",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_Matches_WinnerNextMatchId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "CurrentBracket",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "CurrentRound",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "NumberOfTeams",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "BracketType",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "IsGrandFinal",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "IsSecondGrandFinal",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "LoserNextMatchId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "LoserNextMatchPosition",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "WinnerNextMatchId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "WinnerNextMatchPosition",
                table: "Matches");

            migrationBuilder.RenameColumn(
                name: "IsGrandFinalNeeded",
                table: "Tournaments",
                newName: "IsCompleted");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Tournaments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ScheduledTime",
                table: "Matches",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);
        }
    }
}
