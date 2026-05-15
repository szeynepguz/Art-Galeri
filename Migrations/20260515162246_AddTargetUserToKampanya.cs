using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace art_galeri.Migrations
{
    /// <inheritdoc />
    public partial class AddTargetUserToKampanya : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TargetUserID",
                table: "Kampanyalar",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kampanyalar_TargetUserID",
                table: "Kampanyalar",
                column: "TargetUserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Kampanyalar_Users_TargetUserID",
                table: "Kampanyalar",
                column: "TargetUserID",
                principalTable: "Users",
                principalColumn: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Kampanyalar_Users_TargetUserID",
                table: "Kampanyalar");

            migrationBuilder.DropIndex(
                name: "IX_Kampanyalar_TargetUserID",
                table: "Kampanyalar");

            migrationBuilder.DropColumn(
                name: "TargetUserID",
                table: "Kampanyalar");
        }
    }
}
