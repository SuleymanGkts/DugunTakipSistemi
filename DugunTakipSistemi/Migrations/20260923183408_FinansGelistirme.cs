using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DugunTakipSistemi.Migrations
{
    /// <inheritdoc />
    public partial class FinansGelistirme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MuhatapIsim",
                table: "FinansHareketler",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MusteriId",
                table: "FinansHareketler",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Tedarikciler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sektor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bakiye = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tedarikciler", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinansHareketler_MusteriId",
                table: "FinansHareketler",
                column: "MusteriId");

            migrationBuilder.AddForeignKey(
                name: "FK_FinansHareketler_Musteriler_MusteriId",
                table: "FinansHareketler",
                column: "MusteriId",
                principalTable: "Musteriler",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinansHareketler_Musteriler_MusteriId",
                table: "FinansHareketler");

            migrationBuilder.DropTable(
                name: "Tedarikciler");

            migrationBuilder.DropIndex(
                name: "IX_FinansHareketler_MusteriId",
                table: "FinansHareketler");

            migrationBuilder.DropColumn(
                name: "MuhatapIsim",
                table: "FinansHareketler");

            migrationBuilder.DropColumn(
                name: "MusteriId",
                table: "FinansHareketler");
        }
    }
}
