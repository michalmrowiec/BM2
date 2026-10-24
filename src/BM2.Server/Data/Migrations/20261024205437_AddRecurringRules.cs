using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BM2.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecurringRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "OccurrenceDate",
                table: "Records",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RecurringRuleId",
                table: "Records",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RecurringRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WalletId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TransferAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    Currency = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: false),
                    AccountAmount = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    TransferAmount = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Frequency = table.Column<int>(type: "int", nullable: false),
                    Interval = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    End = table.Column<int>(type: "int", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Count = table.Column<int>(type: "int", nullable: true),
                    Mode = table.Column<int>(type: "int", nullable: false),
                    IsPaused = table.Column<bool>(type: "bit", nullable: false),
                    NextIndex = table.Column<int>(type: "int", nullable: false),
                    NextDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecurringRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecurringRules_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RecurringRuleTag",
                columns: table => new
                {
                    RecurringRuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TagId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecurringRuleTag", x => new { x.RecurringRuleId, x.TagId });
                    table.ForeignKey(
                        name: "FK_RecurringRuleTag_RecurringRules_RecurringRuleId",
                        column: x => x.RecurringRuleId,
                        principalTable: "RecurringRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecurringRuleTag_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Records_RecurringRuleId_OccurrenceDate",
                table: "Records",
                columns: new[] { "RecurringRuleId", "OccurrenceDate" },
                unique: true,
                filter: "[RecurringRuleId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringRules_WalletId_NextDate",
                table: "RecurringRules",
                columns: new[] { "WalletId", "NextDate" });

            migrationBuilder.CreateIndex(
                name: "IX_RecurringRuleTag_TagId",
                table: "RecurringRuleTag",
                column: "TagId");

            migrationBuilder.AddForeignKey(
                name: "FK_Records_RecurringRules_RecurringRuleId",
                table: "Records",
                column: "RecurringRuleId",
                principalTable: "RecurringRules",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Records_RecurringRules_RecurringRuleId",
                table: "Records");

            migrationBuilder.DropTable(
                name: "RecurringRuleTag");

            migrationBuilder.DropTable(
                name: "RecurringRules");

            migrationBuilder.DropIndex(
                name: "IX_Records_RecurringRuleId_OccurrenceDate",
                table: "Records");

            migrationBuilder.DropColumn(
                name: "OccurrenceDate",
                table: "Records");

            migrationBuilder.DropColumn(
                name: "RecurringRuleId",
                table: "Records");
        }
    }
}
