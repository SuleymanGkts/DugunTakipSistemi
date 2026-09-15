using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DugunTakipSistemi.Migrations
{
    /// <inheritdoc />
    public partial class RezervasyonaMekanEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MekanId",
                table: "Rezervasyonlar",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Rezervasyonlar_MekanId",
                table: "Rezervasyonlar",
                column: "MekanId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rezervasyonlar_Mekanlar_MekanId",
                table: "Rezervasyonlar",
                column: "MekanId",
                principalTable: "Mekanlar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rezervasyonlar_Mekanlar_MekanId",
                table: "Rezervasyonlar");

            migrationBuilder.DropIndex(
                name: "IX_Rezervasyonlar_MekanId",
                table: "Rezervasyonlar");

            migrationBuilder.DropColumn(
                name: "MekanId",
                table: "Rezervasyonlar");
        }
    }
}