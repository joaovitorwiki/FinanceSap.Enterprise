using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceSap.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PromoteCustomerToAggregate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_loan_applications_customer_document",
                table: "loan_applications");

            migrationBuilder.DropColumn(
                name: "customer_document",
                table: "loan_applications");

            migrationBuilder.DropColumn(
                name: "customer_full_name",
                table: "loan_applications");

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "loan_applications",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    document = table.Column<string>(type: "VARCHAR(11)", maxLength: 11, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    full_name = table.Column<string>(type: "VARCHAR(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customers", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_loan_applications_CustomerId",
                table: "loan_applications",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_customers_document",
                table: "customers",
                column: "document",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_loan_applications_customers_CustomerId",
                table: "loan_applications",
                column: "CustomerId",
                principalTable: "customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_loan_applications_customers_CustomerId",
                table: "loan_applications");

            migrationBuilder.DropTable(
                name: "customers");

            migrationBuilder.DropIndex(
                name: "IX_loan_applications_CustomerId",
                table: "loan_applications");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "loan_applications");

            migrationBuilder.AddColumn<string>(
                name: "customer_document",
                table: "loan_applications",
                type: "VARCHAR(11)",
                maxLength: 11,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "customer_full_name",
                table: "loan_applications",
                type: "VARCHAR(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_loan_applications_customer_document",
                table: "loan_applications",
                column: "customer_document",
                unique: true);
        }
    }
}
