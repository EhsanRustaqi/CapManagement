using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CapManagement.Server.Migrations
{
    /// <inheritdoc />
    public partial class RenameEmailToCompanyEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompanyEmail",
                table: "Company",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyEmail",
                table: "Company");
        }
    }
}
