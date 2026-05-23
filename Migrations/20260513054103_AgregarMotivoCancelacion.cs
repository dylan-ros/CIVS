using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CIVS.Migrations
{
    /// <inheritdoc />
    public partial class AgregarMotivoCancelacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CitaFechaCancelacion",
                table: "Citas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CitaMotivoCancelacion",
                table: "Citas",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CitaFechaCancelacion",
                table: "Citas");

            migrationBuilder.DropColumn(
                name: "CitaMotivoCancelacion",
                table: "Citas");
        }
    }
}
