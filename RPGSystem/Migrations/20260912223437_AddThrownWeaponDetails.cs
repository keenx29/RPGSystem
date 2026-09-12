using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RPGSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddThrownWeaponDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsThrown",
                table: "Items",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LongRangeFeet",
                table: "Items",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NormalRangeFeet",
                table: "Items",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsThrown",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "LongRangeFeet",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "NormalRangeFeet",
                table: "Items");
        }
    }
}
