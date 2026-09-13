using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RPGSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddStackableInventoryItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsStackable",
                table: "Items",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsStackable",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Items");
        }
    }
}
