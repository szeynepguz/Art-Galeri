using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace art_galeri.Migrations
{
    /// <inheritdoc />
    public partial class UpdateKarsilastirmaModelV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EtkinlikIDler",
                table: "Karsilastirmalar",
                newName: "IDler");

            migrationBuilder.AddColumn<string>(
                name: "Tip",
                table: "Karsilastirmalar",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tip",
                table: "Karsilastirmalar");

            migrationBuilder.RenameColumn(
                name: "IDler",
                table: "Karsilastirmalar",
                newName: "EtkinlikIDler");
        }
    }
}
