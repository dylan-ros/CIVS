using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CIVS.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionsTokenToUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SessionToken",
                table: "Usuarios",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SessionTokenExpiry",
                table: "Usuarios",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SessionToken",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "SessionTokenExpiry",
                table: "Usuarios");
        }
    }
}
