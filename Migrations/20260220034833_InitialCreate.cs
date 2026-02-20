using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CIVS.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pacientes",
                columns: table => new
                {
                    PacienteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PacienteDPI = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    PacienteNombres = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PacienteApellido = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PacienteTelefono = table.Column<string>(type: "nvarchar(18)", maxLength: 18, nullable: false),
                    PacienteNacimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PacienteEstadoCivil = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    PacienteEstado = table.Column<bool>(type: "bit", nullable: false),
                    PacienteFechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pacientes", x => x.PacienteId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pacientes");
        }
    }
}
