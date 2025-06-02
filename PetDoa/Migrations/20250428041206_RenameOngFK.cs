using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetDoa.Migrations
{
    /// <inheritdoc />
    public partial class RenameOngFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Administrator_ONG_ONG_ID",
                table: "Administrator");

            migrationBuilder.DropForeignKey(
                name: "FK_Donation_ONG_ONG_ID",
                table: "Donation");

            migrationBuilder.RenameColumn(
                name: "ONG_ID",
                table: "Donation",
                newName: "OngId");

            migrationBuilder.RenameIndex(
                name: "IX_Donation_ONG_ID",
                table: "Donation",
                newName: "IX_Donation_OngId");

            migrationBuilder.RenameColumn(
                name: "ONG_ID",
                table: "Administrator",
                newName: "OngId");

            migrationBuilder.RenameIndex(
                name: "IX_Administrator_ONG_ID",
                table: "Administrator",
                newName: "IX_Administrator_OngId");

            migrationBuilder.AddForeignKey(
                name: "FK_Administrator_ONG_OngId",
                table: "Administrator",
                column: "OngId",
                principalTable: "ONG",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Donation_ONG_OngId",
                table: "Donation",
                column: "OngId",
                principalTable: "ONG",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Administrator_ONG_OngId",
                table: "Administrator");

            migrationBuilder.DropForeignKey(
                name: "FK_Donation_ONG_OngId",
                table: "Donation");

            migrationBuilder.RenameColumn(
                name: "OngId",
                table: "Donation",
                newName: "ONG_ID");

            migrationBuilder.RenameIndex(
                name: "IX_Donation_OngId",
                table: "Donation",
                newName: "IX_Donation_ONG_ID");

            migrationBuilder.RenameColumn(
                name: "OngId",
                table: "Administrator",
                newName: "ONG_ID");

            migrationBuilder.RenameIndex(
                name: "IX_Administrator_OngId",
                table: "Administrator",
                newName: "IX_Administrator_ONG_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Administrator_ONG_ONG_ID",
                table: "Administrator",
                column: "ONG_ID",
                principalTable: "ONG",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Donation_ONG_ONG_ID",
                table: "Donation",
                column: "ONG_ID",
                principalTable: "ONG",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
