using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CapManagement.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueActiveContractIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Contracts_CarId_CompanyId",
                table: "Contracts",
                columns: new[] { "CarId", "CompanyId" },
                unique: true,
                filter: "[Status] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Contracts_CarId_CompanyId",
                table: "Contracts");
        }
    }
}
