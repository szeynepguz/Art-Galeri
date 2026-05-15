using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace art_galeri.Migrations
{
    /// <inheritdoc />
    public partial class AddOdemeYontemiToRezervasyon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OdemeYontemi",
                table: "Rezervasyonlar",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OdemeYontemi",
                table: "Rezervasyonlar");
        }
    }
}
