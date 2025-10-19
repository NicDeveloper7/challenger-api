using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Challenger.Infra.Migrations
{
    /// <inheritdoc />
    public partial class AddHighlightedMotorcycles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "highlighted_motorcycles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MotorcycleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Plate = table.Column<string>(type: "text", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Model = table.Column<string>(type: "text", nullable: false),
                    HighlightedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_highlighted_motorcycles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Motorcycles_Plate",
                table: "Motorcycles",
                column: "Plate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_highlighted_motorcycles_MotorcycleId",
                table: "highlighted_motorcycles",
                column: "MotorcycleId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "highlighted_motorcycles");

            migrationBuilder.DropIndex(
                name: "IX_Motorcycles_Plate",
                table: "Motorcycles");
        }
    }
}
