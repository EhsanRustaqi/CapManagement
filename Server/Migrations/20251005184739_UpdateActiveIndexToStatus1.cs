using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CapManagement.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateActiveIndexToStatus1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Contracts_CarId_CompanyId",
                table: "Contracts");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_CarId_CompanyId",
                table: "Contracts",
                columns: new[] { "CarId", "CompanyId" },
                unique: true,
                filter: "[Status] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Contracts_CarId_CompanyId",
                table: "Contracts");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_CarId_CompanyId",
                table: "Contracts",
                columns: new[] { "CarId", "CompanyId" },
                unique: true,
                filter: "[Status] = 0");
        }
    }
}
