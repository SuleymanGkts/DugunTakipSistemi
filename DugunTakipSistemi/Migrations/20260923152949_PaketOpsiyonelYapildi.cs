using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DugunTakipSistemi.Migrations
{
    /// <inheritdoc />
    public partial class PaketOpsiyonelYapildi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rezervasyonlar_Paketler_PaketId",
                table: "Rezervasyonlar");

            migrationBuilder.AlterColumn<int>(
                name: "PaketId",
                table: "Rezervasyonlar",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Rezervasyonlar_Paketler_PaketId",
                table: "Rezervasyonlar",
                column: "PaketId",
                principalTable: "Paketler",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rezervasyonlar_Paketler_PaketId",
                table: "Rezervasyonlar");

            migrationBuilder.AlterColumn<int>(
                name: "PaketId",
                table: "Rezervasyonlar",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Rezervasyonlar_Paketler_PaketId",
                table: "Rezervasyonlar",
                column: "PaketId",
                principalTable: "Paketler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
