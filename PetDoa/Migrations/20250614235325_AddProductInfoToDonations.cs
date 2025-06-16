using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetDoa.Migrations
{
    /// <inheritdoc />
    public partial class AddProductInfoToDonations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "Donation",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "Donation",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "Donation");

            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "Donation");
        }
    }
}
