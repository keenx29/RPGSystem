using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RPGSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddCharacterSessionState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Conditions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Conditions_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeatureResources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Current = table.Column<int>(type: "int", nullable: false),
                    Max = table.Column<int>(type: "int", nullable: false),
                    ResetType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatureResources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeatureResources_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeatureStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FeatureName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UsesRemaining = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatureStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeatureStates_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Conditions_CharacterId_Type",
                table: "Conditions",
                columns: new[] { "CharacterId", "Type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FeatureResources_CharacterId_Name",
                table: "FeatureResources",
                columns: new[] { "CharacterId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FeatureStates_CharacterId_FeatureName",
                table: "FeatureStates",
                columns: new[] { "CharacterId", "FeatureName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Conditions");

            migrationBuilder.DropTable(
                name: "FeatureResources");

            migrationBuilder.DropTable(
                name: "FeatureStates");
        }
    }
}
