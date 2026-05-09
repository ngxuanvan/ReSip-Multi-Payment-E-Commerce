using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResipWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddVnpFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasIpn",
                table: "VnPayTransactions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasReturn",
                table: "VnPayTransactions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasIpn",
                table: "VnPayTransactions");

            migrationBuilder.DropColumn(
                name: "HasReturn",
                table: "VnPayTransactions");
        }
    }
}
