using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Challenger.Infra.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRentalModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalPrice",
                table: "Rentals",
                newName: "TotalAmount");

            migrationBuilder.RenameColumn(
                name: "PlanType",
                table: "Rentals",
                newName: "PlanDays");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "Rentals",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "DailyPrice",
                table: "Rentals",
                newName: "FinalAmount");

            migrationBuilder.RenameColumn(
                name: "AdditionalFee",
                table: "Rentals",
                newName: "ExtraFee");

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualEndDate",
                table: "Rentals",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DailyRate",
                table: "Rentals",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Rentals_StartDate_NextDay",
                table: "Rentals",
                sql: "\"StartDate\" = date_trunc('day', \"CreatedAt\") + interval '1 day'");

            migrationBuilder.CreateIndex(
                name: "IX_DeliverymanProfiles_CnhNumber",
                table: "DeliverymanProfiles",
                column: "CnhNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeliverymanProfiles_Cnpj",
                table: "DeliverymanProfiles",
                column: "Cnpj",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Rentals_StartDate_NextDay",
                table: "Rentals");

            migrationBuilder.DropIndex(
                name: "IX_DeliverymanProfiles_CnhNumber",
                table: "DeliverymanProfiles");

            migrationBuilder.DropIndex(
                name: "IX_DeliverymanProfiles_Cnpj",
                table: "DeliverymanProfiles");

            migrationBuilder.DropColumn(
                name: "ActualEndDate",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "DailyRate",
                table: "Rentals");

            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "Rentals",
                newName: "TotalPrice");

            migrationBuilder.RenameColumn(
                name: "PlanDays",
                table: "Rentals",
                newName: "PlanType");

            migrationBuilder.RenameColumn(
                name: "FinalAmount",
                table: "Rentals",
                newName: "DailyPrice");

            migrationBuilder.RenameColumn(
                name: "ExtraFee",
                table: "Rentals",
                newName: "AdditionalFee");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Rentals",
                newName: "EndDate");
        }
    }
}
