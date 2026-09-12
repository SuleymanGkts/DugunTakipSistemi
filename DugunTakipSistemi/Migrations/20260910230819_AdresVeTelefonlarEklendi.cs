using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DugunTakipSistemi.Migrations
{
    /// <inheritdoc />
    public partial class AdresVeTelefonlarEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Adres",
                table: "Musteriler",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DamatAdSoyad",
                table: "Musteriler",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DamatTelefon",
                table: "Musteriler",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Musteriler",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GelinAdSoyad",
                table: "Musteriler",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GelinTelefon",
                table: "Musteriler",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Adres",
                table: "Musteriler");

            migrationBuilder.DropColumn(
                name: "DamatAdSoyad",
                table: "Musteriler");

            migrationBuilder.DropColumn(
                name: "DamatTelefon",
                table: "Musteriler");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Musteriler");

            migrationBuilder.DropColumn(
                name: "GelinAdSoyad",
                table: "Musteriler");

            migrationBuilder.DropColumn(
                name: "GelinTelefon",
                table: "Musteriler");
        }
    }
}
