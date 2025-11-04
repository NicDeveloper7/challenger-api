using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Challenger.Infra.Migrations
{
    /// <inheritdoc />
    public partial class MakeMotorcycleNullableAndFixConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Rentals_StartDate_NextDay",
                table: "Rentals");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Rentals_StartDate_NextDay",
                table: "Rentals",
                sql: "\"StartDate\" = (date_trunc('day', \"CreatedAt\") + interval '1 day')::date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Rentals_StartDate_NextDay",
                table: "Rentals");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Rentals_StartDate_NextDay",
                table: "Rentals",
                sql: "\"StartDate\" = date_trunc('day', \"CreatedAt\") + interval '1 day'");
        }
    }
}
