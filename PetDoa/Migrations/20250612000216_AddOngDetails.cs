using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetDoa.Migrations
{
    /// <inheritdoc />
    public partial class AddOngDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cause",
                table: "ONG",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "ONG",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Cnpj",
                table: "ONG",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Number",
                table: "ONG",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "ONG",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Street",
                table: "ONG",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                table: "ONG",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cause",
                table: "ONG");

            migrationBuilder.DropColumn(
                name: "City",
                table: "ONG");

            migrationBuilder.DropColumn(
                name: "Cnpj",
                table: "ONG");

            migrationBuilder.DropColumn(
                name: "Number",
                table: "ONG");

            migrationBuilder.DropColumn(
                name: "State",
                table: "ONG");

            migrationBuilder.DropColumn(
                name: "Street",
                table: "ONG");

            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "ONG");
        }
    }
}
