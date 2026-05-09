using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResipWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueMaDonHang : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MaDonHang",
                table: "DonHangs",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_DonHangs_MaDonHang_Unique",
                table: "DonHangs",
                column: "MaDonHang",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DonHangs_MaDonHang_Unique",
                table: "DonHangs");

            migrationBuilder.AlterColumn<string>(
                name: "MaDonHang",
                table: "DonHangs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
