using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetDoa.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusAndGatewayIdToDonation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GatewayPaymentId",
                table: "Donation",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Donation",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GatewayPaymentId",
                table: "Donation");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Donation");
        }
    }
}
