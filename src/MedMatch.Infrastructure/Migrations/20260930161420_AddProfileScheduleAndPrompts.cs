using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedMatch.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileScheduleAndPrompts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Mevcut satırlar için defensive default: her enum'un ilk geçerli değeri (0 tanımsızdır).
            migrationBuilder.AddColumn<int>(
                name: "CareerStage",
                table: "Profiles",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "MandatoryService",
                table: "Profiles",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "NightShiftLoad",
                table: "Profiles",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "Relocation",
                table: "Profiles",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "WorkSchedule",
                table: "Profiles",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "Prompts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    PromptKey = table.Column<int>(type: "integer", nullable: false),
                    Answer = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prompts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prompts_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Prompts_ProfileId_PromptKey",
                table: "Prompts",
                columns: new[] { "ProfileId", "PromptKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Prompts");

            migrationBuilder.DropColumn(
                name: "CareerStage",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "MandatoryService",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "NightShiftLoad",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "Relocation",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "WorkSchedule",
                table: "Profiles");
        }
    }
}
