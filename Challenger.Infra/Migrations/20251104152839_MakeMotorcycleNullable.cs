using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Challenger.Infra.Migrations
{
    /// <inheritdoc />
    public partial class MakeMotorcycleNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rentals_Motorcycles_MotorcycleId",
                table: "Rentals");

            migrationBuilder.AlterColumn<Guid>(
                name: "MotorcycleId",
                table: "Rentals",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Rentals_Motorcycles_MotorcycleId",
                table: "Rentals",
                column: "MotorcycleId",
                principalTable: "Motorcycles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rentals_Motorcycles_MotorcycleId",
                table: "Rentals");

            migrationBuilder.AlterColumn<Guid>(
                name: "MotorcycleId",
                table: "Rentals",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Rentals_Motorcycles_MotorcycleId",
                table: "Rentals",
                column: "MotorcycleId",
                principalTable: "Motorcycles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
