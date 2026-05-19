using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace art_galeri.Migrations
{
    /// <inheritdoc />
    public partial class EtkinlikCokluTarih : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EtkinlikTarihID",
                table: "Rezervasyonlar",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EtkinlikTarihler",
                columns: table => new
                {
                    EtkinlikTarihID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EtkinlikID = table.Column<int>(type: "integer", nullable: false),
                    Tarih = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Saat = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Kapasite = table.Column<int>(type: "integer", nullable: false),
                    RezervasyonSayisi = table.Column<int>(type: "integer", nullable: false),
                    Aktif = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EtkinlikTarihler", x => x.EtkinlikTarihID);
                    table.ForeignKey(
                        name: "FK_EtkinlikTarihler_Etkinlikler_EtkinlikID",
                        column: x => x.EtkinlikID,
                        principalTable: "Etkinlikler",
                        principalColumn: "EtkinlikID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Rezervasyonlar_EtkinlikTarihID",
                table: "Rezervasyonlar",
                column: "EtkinlikTarihID");

            migrationBuilder.CreateIndex(
                name: "IX_EtkinlikTarihler_EtkinlikID",
                table: "EtkinlikTarihler",
                column: "EtkinlikID");

            migrationBuilder.AddForeignKey(
                name: "FK_Rezervasyonlar_EtkinlikTarihler_EtkinlikTarihID",
                table: "Rezervasyonlar",
                column: "EtkinlikTarihID",
                principalTable: "EtkinlikTarihler",
                principalColumn: "EtkinlikTarihID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rezervasyonlar_EtkinlikTarihler_EtkinlikTarihID",
                table: "Rezervasyonlar");

            migrationBuilder.DropTable(
                name: "EtkinlikTarihler");

            migrationBuilder.DropIndex(
                name: "IX_Rezervasyonlar_EtkinlikTarihID",
                table: "Rezervasyonlar");

            migrationBuilder.DropColumn(
                name: "EtkinlikTarihID",
                table: "Rezervasyonlar");
        }
    }
}
