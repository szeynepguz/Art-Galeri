using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace art_galeri.Migrations
{
    /// <inheritdoc />
    public partial class TamSistem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Artworks",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Artworks",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Artworks",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Aktif",
                table: "Artworks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "BegeniSayisi",
                table: "Artworks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GoruntulenmeSayisi",
                table: "Artworks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Kategori",
                table: "Artworks",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "YorumSayisi",
                table: "Artworks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DestekTalepleri",
                columns: table => new
                {
                    TalepID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserID = table.Column<int>(type: "integer", nullable: true),
                    Konu = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Mesaj = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Durum = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    YoneticiYaniti = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    YanitTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DestekTalepleri", x => x.TalepID);
                    table.ForeignKey(
                        name: "FK_DestekTalepleri_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Etkinlikler",
                columns: table => new
                {
                    EtkinlikID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ad = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Aciklama = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Tarih = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Saat = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Konum = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Ucret = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Kapasite = table.Column<int>(type: "integer", nullable: false),
                    RezervasyonSayisi = table.Column<int>(type: "integer", nullable: false),
                    OrtalamaPuan = table.Column<double>(type: "double precision", nullable: false),
                    GorselUrl = table.Column<string>(type: "text", nullable: true),
                    Kategori = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Aktif = table.Column<bool>(type: "boolean", nullable: false),
                    EgitmenID = table.Column<int>(type: "integer", nullable: true),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Etkinlikler", x => x.EtkinlikID);
                    table.ForeignKey(
                        name: "FK_Etkinlikler_Users_EgitmenID",
                        column: x => x.EgitmenID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Favoriler",
                columns: table => new
                {
                    FavoriID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserID = table.Column<int>(type: "integer", nullable: false),
                    ArtworkID = table.Column<int>(type: "integer", nullable: true),
                    EklenmeTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favoriler", x => x.FavoriID);
                    table.ForeignKey(
                        name: "FK_Favoriler_Artworks_ArtworkID",
                        column: x => x.ArtworkID,
                        principalTable: "Artworks",
                        principalColumn: "ArtworkID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Favoriler_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Kampanyalar",
                columns: table => new
                {
                    KampanyaID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ad = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Aciklama = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IndirimOrani = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    KuponKodu = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BaslangicTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BitisTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Aktif = table.Column<bool>(type: "boolean", nullable: false),
                    HedefRolID = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kampanyalar", x => x.KampanyaID);
                    table.ForeignKey(
                        name: "FK_Kampanyalar_Roller_HedefRolID",
                        column: x => x.HedefRolID,
                        principalTable: "Roller",
                        principalColumn: "RolID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Rezervasyonlar",
                columns: table => new
                {
                    RezervasyonID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserID = table.Column<int>(type: "integer", nullable: false),
                    EtkinlikID = table.Column<int>(type: "integer", nullable: false),
                    KatilimciSayisi = table.Column<int>(type: "integer", nullable: false),
                    RezervasyonTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Durum = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Notlar = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ToplamTutar = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rezervasyonlar", x => x.RezervasyonID);
                    table.ForeignKey(
                        name: "FK_Rezervasyonlar_Etkinlikler_EtkinlikID",
                        column: x => x.EtkinlikID,
                        principalTable: "Etkinlikler",
                        principalColumn: "EtkinlikID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Rezervasyonlar_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Yorumlar",
                columns: table => new
                {
                    YorumID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserID = table.Column<int>(type: "integer", nullable: false),
                    ArtworkID = table.Column<int>(type: "integer", nullable: true),
                    EtkinlikID = table.Column<int>(type: "integer", nullable: true),
                    Icerik = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Puan = table.Column<int>(type: "integer", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Onaylandi = table.Column<bool>(type: "boolean", nullable: false),
                    YoneticiYaniti = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    YanitTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FaydaliBulma = table.Column<int>(type: "integer", nullable: false),
                    Dogrulanmis = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Yorumlar", x => x.YorumID);
                    table.ForeignKey(
                        name: "FK_Yorumlar_Artworks_ArtworkID",
                        column: x => x.ArtworkID,
                        principalTable: "Artworks",
                        principalColumn: "ArtworkID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Yorumlar_Etkinlikler_EtkinlikID",
                        column: x => x.EtkinlikID,
                        principalTable: "Etkinlikler",
                        principalColumn: "EtkinlikID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Yorumlar_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Siparisler",
                columns: table => new
                {
                    SiparisID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserID = table.Column<int>(type: "integer", nullable: false),
                    ArtworkID = table.Column<int>(type: "integer", nullable: true),
                    Tutar = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    OdemeYontemi = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Durum = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SiparisTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Adres = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    KampanyaID = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Siparisler", x => x.SiparisID);
                    table.ForeignKey(
                        name: "FK_Siparisler_Artworks_ArtworkID",
                        column: x => x.ArtworkID,
                        principalTable: "Artworks",
                        principalColumn: "ArtworkID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Siparisler_Kampanyalar_KampanyaID",
                        column: x => x.KampanyaID,
                        principalTable: "Kampanyalar",
                        principalColumn: "KampanyaID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Siparisler_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Artworks_ArtistID",
                table: "Artworks",
                column: "ArtistID");

            migrationBuilder.CreateIndex(
                name: "IX_DestekTalepleri_UserID",
                table: "DestekTalepleri",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Etkinlikler_EgitmenID",
                table: "Etkinlikler",
                column: "EgitmenID");

            migrationBuilder.CreateIndex(
                name: "IX_Favoriler_ArtworkID",
                table: "Favoriler",
                column: "ArtworkID");

            migrationBuilder.CreateIndex(
                name: "IX_Favoriler_UserID",
                table: "Favoriler",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Kampanyalar_HedefRolID",
                table: "Kampanyalar",
                column: "HedefRolID");

            migrationBuilder.CreateIndex(
                name: "IX_Kampanyalar_KuponKodu",
                table: "Kampanyalar",
                column: "KuponKodu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rezervasyonlar_EtkinlikID",
                table: "Rezervasyonlar",
                column: "EtkinlikID");

            migrationBuilder.CreateIndex(
                name: "IX_Rezervasyonlar_UserID",
                table: "Rezervasyonlar",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Siparisler_ArtworkID",
                table: "Siparisler",
                column: "ArtworkID");

            migrationBuilder.CreateIndex(
                name: "IX_Siparisler_KampanyaID",
                table: "Siparisler",
                column: "KampanyaID");

            migrationBuilder.CreateIndex(
                name: "IX_Siparisler_UserID",
                table: "Siparisler",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Yorumlar_ArtworkID",
                table: "Yorumlar",
                column: "ArtworkID");

            migrationBuilder.CreateIndex(
                name: "IX_Yorumlar_EtkinlikID",
                table: "Yorumlar",
                column: "EtkinlikID");

            migrationBuilder.CreateIndex(
                name: "IX_Yorumlar_UserID",
                table: "Yorumlar",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Artworks_Users_ArtistID",
                table: "Artworks",
                column: "ArtistID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Artworks_Users_ArtistID",
                table: "Artworks");

            migrationBuilder.DropTable(
                name: "DestekTalepleri");

            migrationBuilder.DropTable(
                name: "Favoriler");

            migrationBuilder.DropTable(
                name: "Rezervasyonlar");

            migrationBuilder.DropTable(
                name: "Siparisler");

            migrationBuilder.DropTable(
                name: "Yorumlar");

            migrationBuilder.DropTable(
                name: "Kampanyalar");

            migrationBuilder.DropTable(
                name: "Etkinlikler");

            migrationBuilder.DropIndex(
                name: "IX_Artworks_ArtistID",
                table: "Artworks");

            migrationBuilder.DropColumn(
                name: "Aktif",
                table: "Artworks");

            migrationBuilder.DropColumn(
                name: "BegeniSayisi",
                table: "Artworks");

            migrationBuilder.DropColumn(
                name: "GoruntulenmeSayisi",
                table: "Artworks");

            migrationBuilder.DropColumn(
                name: "Kategori",
                table: "Artworks");

            migrationBuilder.DropColumn(
                name: "YorumSayisi",
                table: "Artworks");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Artworks",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Artworks",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Artworks",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);
        }
    }
}
