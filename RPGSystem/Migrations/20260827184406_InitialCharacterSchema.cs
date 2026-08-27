using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RPGSystem.Migrations
{
    /// <inheritdoc />
    public partial class InitialCharacterSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Characters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClassType = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    MaxHP = table.Column<int>(type: "int", nullable: false),
                    CurrentHP = table.Column<int>(type: "int", nullable: false),
                    MovementSpeed = table.Column<int>(type: "int", nullable: false),
                    Race = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Background = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Alignment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PersonalityTraits = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ideals = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Bonds = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Flaws = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HitDiceRemaining = table.Column<int>(type: "int", nullable: false),
                    PendingAbilityScoreImprovementPoints = table.Column<int>(type: "int", nullable: false),
                    DeathSaveSuccesses = table.Column<int>(type: "int", nullable: false),
                    DeathSaveFailures = table.Column<int>(type: "int", nullable: false),
                    IsStable = table.Column<bool>(type: "bit", nullable: false),
                    IsDead = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Characters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Abilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    IsSavingThrowProficient = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Abilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Abilities_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Skills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    RelatedAbilityType = table.Column<int>(type: "int", nullable: false),
                    IsProficient = table.Column<bool>(type: "bit", nullable: false),
                    IsExpertise = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Skills_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Abilities_CharacterId_Type",
                table: "Abilities",
                columns: new[] { "CharacterId", "Type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Skills_CharacterId_Type",
                table: "Skills",
                columns: new[] { "CharacterId", "Type" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Abilities");

            migrationBuilder.DropTable(
                name: "Skills");

            migrationBuilder.DropTable(
                name: "Characters");
        }
    }
}
