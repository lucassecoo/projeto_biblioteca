using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace biblioteca.Migrations
{
    /// <inheritdoc />
    public partial class AjustandoTabelas1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataDevolucaoPrevista",
                table: "Emprestimos",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataDevolucaoPrevista",
                table: "Emprestimos");
        }
    }
}
