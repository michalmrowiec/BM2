using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BM2.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecordPairs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PairAccountId",
                table: "RecurringRules",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PairAmount",
                table: "RecurringRules",
                type: "decimal(19,4)",
                precision: 19,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PairCategoryId",
                table: "RecurringRules",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PairWalletId",
                table: "RecurringRules",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PairAccountId",
                table: "Records",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PairAmount",
                table: "Records",
                type: "decimal(19,4)",
                precision: 19,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PairCategoryId",
                table: "Records",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PairId",
                table: "Records",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PairWalletId",
                table: "Records",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Records_PairId",
                table: "Records",
                column: "PairId",
                filter: "[PairId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Records_PairId",
                table: "Records");

            migrationBuilder.DropColumn(
                name: "PairAccountId",
                table: "RecurringRules");

            migrationBuilder.DropColumn(
                name: "PairAmount",
                table: "RecurringRules");

            migrationBuilder.DropColumn(
                name: "PairCategoryId",
                table: "RecurringRules");

            migrationBuilder.DropColumn(
                name: "PairWalletId",
                table: "RecurringRules");

            migrationBuilder.DropColumn(
                name: "PairAccountId",
                table: "Records");

            migrationBuilder.DropColumn(
                name: "PairAmount",
                table: "Records");

            migrationBuilder.DropColumn(
                name: "PairCategoryId",
                table: "Records");

            migrationBuilder.DropColumn(
                name: "PairId",
                table: "Records");

            migrationBuilder.DropColumn(
                name: "PairWalletId",
                table: "Records");
        }
    }
}
