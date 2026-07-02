using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BM2.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPeriodicRecordDefinitionCols : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HangfireJobId",
                table: "PeriodicRecordDefinitions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextExecutionAt",
                table: "PeriodicRecordDefinitions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Periodicity",
                table: "PeriodicRecordDefinitions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "PeriodicRecordDefinitions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HangfireJobId",
                table: "PeriodicRecordDefinitions");

            migrationBuilder.DropColumn(
                name: "NextExecutionAt",
                table: "PeriodicRecordDefinitions");

            migrationBuilder.DropColumn(
                name: "Periodicity",
                table: "PeriodicRecordDefinitions");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "PeriodicRecordDefinitions");
        }
    }
}
