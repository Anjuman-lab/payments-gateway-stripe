using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payments.API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCardConnectColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthCode",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "RetRef",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Token",
                table: "Payments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthCode",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RetRef",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Token",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
