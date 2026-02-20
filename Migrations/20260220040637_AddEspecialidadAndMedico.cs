using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CIVS.Migrations
{
    /// <inheritdoc />
    public partial class AddEspecialidadAndMedico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Especialidades",
                columns: table => new
                {
                    EspecialidadId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EspecialidadNombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    EspecialidadEstado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Especialidades", x => x.EspecialidadId);
                });

            migrationBuilder.CreateTable(
                name: "Medicos",
                columns: table => new
                {
                    MedicoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EspecialidadId = table.Column<int>(type: "int", nullable: false),
                    MedicoNombres = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MedicoApellidos = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MedicoColegiado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MedicoTelefono = table.Column<string>(type: "nvarchar(18)", maxLength: 18, nullable: false),
                    MedicoEmail = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    MedicoEstado = table.Column<bool>(type: "bit", nullable: false),
                    MedicoFechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medicos", x => x.MedicoId);
                    table.ForeignKey(
                        name: "FK_Medicos_Especialidades_EspecialidadId",
                        column: x => x.EspecialidadId,
                        principalTable: "Especialidades",
                        principalColumn: "EspecialidadId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Medicos_EspecialidadId",
                table: "Medicos",
                column: "EspecialidadId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Medicos");

            migrationBuilder.DropTable(
                name: "Especialidades");
        }
    }
}
