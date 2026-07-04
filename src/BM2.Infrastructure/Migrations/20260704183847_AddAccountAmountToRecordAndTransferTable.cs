using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BM2.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountAmountToRecordAndTransferTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AccountAmount",
                table: "RecordTemplates",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AccountAmount",
                table: "Records",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "AccountRecordTransferId",
                table: "Records",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountAmount",
                table: "RecordTemplates");

            migrationBuilder.DropColumn(
                name: "AccountAmount",
                table: "Records");

            migrationBuilder.DropColumn(
                name: "AccountRecordTransferId",
                table: "Records");
        }
    }
}
