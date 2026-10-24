using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BM2.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTemplatesCategoryIconsTagColors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Tags",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "Categories",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            // Existing starter categories get the same icons as new wallets.
            foreach (var (name, icon) in new[]
            {
                ("Groceries", "basket"), ("Eating out", "food"), ("Transport", "car"), ("Home", "home"), ("Bills", "receipt"),
                ("Health", "pill"), ("Shopping", "cart"), ("Entertainment", "film"), ("Other", "dots"), ("Salary", "briefcase"),
                ("Other income", "piggy"),
            })
            {
                migrationBuilder.Sql($"UPDATE [Categories] SET [Icon] = N'{icon}' WHERE [Name] = N'{name}' AND [Icon] IS NULL");
            }

            migrationBuilder.CreateTable(
                name: "Templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WalletId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TransferAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: true),
                    Currency = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsPinned = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Templates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Templates_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TemplateTag",
                columns: table => new
                {
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TagId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateTag", x => new { x.TemplateId, x.TagId });
                    table.ForeignKey(
                        name: "FK_TemplateTag_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TemplateTag_Templates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "Templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Templates_WalletId",
                table: "Templates",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateTag_TagId",
                table: "TemplateTag",
                column: "TagId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TemplateTag");

            migrationBuilder.DropTable(
                name: "Templates");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "Categories");
        }
    }
}
