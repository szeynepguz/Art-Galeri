using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace art_galeri.Migrations
{
    /// <inheritdoc />
    public partial class RolSistemi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ad",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "RolID",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Sifre",
                table: "Users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Soyad",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "EgitmenProfiller",
                columns: table => new
                {
                    ProfilID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserID = table.Column<int>(type: "integer", nullable: false),
                    UzmanlikAlani = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Biyografi = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DeneyimYili = table.Column<int>(type: "integer", nullable: true),
                    SertifikaUrl = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    KatilimTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EgitmenProfiller", x => x.ProfilID);
                    table.ForeignKey(
                        name: "FK_EgitmenProfiller_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MusteriProfiller",
                columns: table => new
                {
                    ProfilID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserID = table.Column<int>(type: "integer", nullable: false),
                    Adres = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Telefon = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IlgiAlanlari = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    KatilimTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusteriProfiller", x => x.ProfilID);
                    table.ForeignKey(
                        name: "FK_MusteriProfiller_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Roller",
                columns: table => new
                {
                    RolID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RolAdi = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Aciklama = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roller", x => x.RolID);
                });

            migrationBuilder.CreateTable(
                name: "SanatciProfiller",
                columns: table => new
                {
                    ProfilID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserID = table.Column<int>(type: "integer", nullable: false),
                    Ozgecmis = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PortfolyoLinki = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SanatDali = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ProfilFotoUrl = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    KatilimTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SanatciProfiller", x => x.ProfilID);
                    table.ForeignKey(
                        name: "FK_SanatciProfiller_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Roller",
                columns: new[] { "RolID", "Aciklama", "RolAdi" },
                values: new object[,]
                {
                    { 1, "Eser inceleyen ve satın alan kullanıcılar", "Musteri" },
                    { 2, "Sistem yöneticisi - tam yetki", "Yonetici" },
                    { 3, "Atölye ve workshop yöneten eğitmenler", "Egitmen" },
                    { 4, "Eser yükleyen ve sergileyen sanatçılar", "Sanatci" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_RolID",
                table: "Users",
                column: "RolID");

            migrationBuilder.CreateIndex(
                name: "IX_EgitmenProfiller_UserID",
                table: "EgitmenProfiller",
                column: "UserID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MusteriProfiller_UserID",
                table: "MusteriProfiller",
                column: "UserID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SanatciProfiller_UserID",
                table: "SanatciProfiller",
                column: "UserID",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roller_RolID",
                table: "Users",
                column: "RolID",
                principalTable: "Roller",
                principalColumn: "RolID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roller_RolID",
                table: "Users");

            migrationBuilder.DropTable(
                name: "EgitmenProfiller");

            migrationBuilder.DropTable(
                name: "MusteriProfiller");

            migrationBuilder.DropTable(
                name: "Roller");

            migrationBuilder.DropTable(
                name: "SanatciProfiller");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_RolID",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Ad",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RolID",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Sifre",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Soyad",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Users",
                type: "text",
                nullable: true);
        }
    }
}
