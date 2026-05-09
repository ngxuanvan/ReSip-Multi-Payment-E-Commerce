using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResipWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddPayPalTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PayPalTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PayPalOrderId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PayerID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PayerEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AmountUsd = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayPalTransactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SePayTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SepayTxnId = table.Column<long>(type: "bigint", nullable: false),
                    OrderCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransferType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransactionDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RawGateway = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SePayTransactions", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PayPalTransactions");

            migrationBuilder.DropTable(
                name: "SePayTransactions");
        }
    }
}
