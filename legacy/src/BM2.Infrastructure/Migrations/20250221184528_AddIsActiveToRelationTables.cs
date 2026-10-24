using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BM2.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToRelationTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "WalletTagRelations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "WalletCategoryRelations",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "WalletTagRelations");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "WalletCategoryRelations");
        }
    }
}
