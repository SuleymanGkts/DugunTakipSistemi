using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DugunTakipSistemi.Migrations
{
    /// <inheritdoc />
    public partial class KisiSayisiEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "KisiSayisi",
                table: "Rezervasyonlar",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KisiSayisi",
                table: "Rezervasyonlar");
        }
    }
}
