using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CIVS.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateAll_CIVS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Diagnosticos",
                columns: table => new
                {
                    DiagnosticoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiagnosticoCodigo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DiagnosticoNombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DiagnosticoEstado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Diagnosticos", x => x.DiagnosticoId);
                });

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
                name: "Examenes",
                columns: table => new
                {
                    ExamenId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamenNombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ExamenDescripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExamenPrecio = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    ExamenEstado = table.Column<bool>(type: "bit", nullable: false),
                    ExamenFechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Examenes", x => x.ExamenId);
                });

            migrationBuilder.CreateTable(
                name: "Medicamentos",
                columns: table => new
                {
                    MedicamentoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MedicamentoNombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MedicamentoPresentacion = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    MedicamentoConcentracion = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    MedicamentoUnidad = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    MedicamentoPrecio = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MedicamentoEstado = table.Column<bool>(type: "bit", nullable: false),
                    MedicamentoFechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medicamentos", x => x.MedicamentoId);
                });

            migrationBuilder.CreateTable(
                name: "MetodoPagos",
                columns: table => new
                {
                    MetodoPagoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MetodoPagoNombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MetodoPagoDescripcion = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    MetodoPagoEstado = table.Column<bool>(type: "bit", nullable: false),
                    MetodoPagoFechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetodoPagos", x => x.MetodoPagoId);
                });

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

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RolId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RolNombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RolDescripcion = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    RolEstado = table.Column<bool>(type: "bit", nullable: false),
                    RolFechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RolId);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioUsername = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UsuarioEmail = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    UsuarioPasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    UsuarioEstado = table.Column<bool>(type: "bit", nullable: false),
                    UsuarioFechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioNombres = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    UsuarioApellidos = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.UsuarioId);
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

            migrationBuilder.CreateTable(
                name: "Inventarios",
                columns: table => new
                {
                    InventarioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MedicamentoId = table.Column<int>(type: "int", nullable: false),
                    StockActual = table.Column<int>(type: "int", nullable: false),
                    StockMinimo = table.Column<int>(type: "int", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InventarioEstado = table.Column<bool>(type: "bit", nullable: false),
                    InventarioFechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventarios", x => x.InventarioId);
                    table.ForeignKey(
                        name: "FK_Inventarios_Medicamentos_MedicamentoId",
                        column: x => x.MedicamentoId,
                        principalTable: "Medicamentos",
                        principalColumn: "MedicamentoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Auditorias",
                columns: table => new
                {
                    AuditoriaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    AuditoriaAccion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AuditoriaEntidad = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    AuditoriaEntidadId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AuditoriaDescripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AuditoriaFecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AuditoriaEstado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auditorias", x => x.AuditoriaId);
                    table.ForeignKey(
                        name: "FK_Auditorias_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId");
                });

            migrationBuilder.CreateTable(
                name: "UsuarioRoles",
                columns: table => new
                {
                    UsuarioRolId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    RolId = table.Column<int>(type: "int", nullable: false),
                    UsuarioRolFechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioRolEstado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioRoles", x => x.UsuarioRolId);
                    table.ForeignKey(
                        name: "FK_UsuarioRoles_Roles_RolId",
                        column: x => x.RolId,
                        principalTable: "Roles",
                        principalColumn: "RolId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsuarioRoles_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Citas",
                columns: table => new
                {
                    CitaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PacienteId = table.Column<int>(type: "int", nullable: false),
                    MedicoId = table.Column<int>(type: "int", nullable: false),
                    CitaFechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CitaFechafin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CitaMotivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EstadoCita = table.Column<int>(type: "int", nullable: false),
                    CitaFechaCreada = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Citas", x => x.CitaId);
                    table.ForeignKey(
                        name: "FK_Citas_Medicos_MedicoId",
                        column: x => x.MedicoId,
                        principalTable: "Medicos",
                        principalColumn: "MedicoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Citas_Pacientes_PacienteId",
                        column: x => x.PacienteId,
                        principalTable: "Pacientes",
                        principalColumn: "PacienteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HorarioMedicos",
                columns: table => new
                {
                    HorarioMedicoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MedicoId = table.Column<int>(type: "int", nullable: false),
                    MedicoHorarioInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MedicoHorarioFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MedicoHorarioDisponible = table.Column<bool>(type: "bit", nullable: false),
                    HorarioNota = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    HorarioFechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorarioMedicos", x => x.HorarioMedicoId);
                    table.ForeignKey(
                        name: "FK_HorarioMedicos_Medicos_MedicoId",
                        column: x => x.MedicoId,
                        principalTable: "Medicos",
                        principalColumn: "MedicoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventarioMovimientos",
                columns: table => new
                {
                    InventarioMovimientoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventarioId = table.Column<int>(type: "int", nullable: false),
                    MovimientoTipo = table.Column<int>(type: "int", nullable: false),
                    MovimientoCantidad = table.Column<int>(type: "int", nullable: false),
                    MovimientoMotivo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MovimientoFecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MovimientoEstado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventarioMovimientos", x => x.InventarioMovimientoId);
                    table.ForeignKey(
                        name: "FK_InventarioMovimientos_Inventarios_InventarioId",
                        column: x => x.InventarioId,
                        principalTable: "Inventarios",
                        principalColumn: "InventarioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Consultas",
                columns: table => new
                {
                    ConsultaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CitaId = table.Column<int>(type: "int", nullable: false),
                    ConsultaSignosVitales = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ConsultaNotasClinicas = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ConsultaPlanTratamiento = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ConsultaEstado = table.Column<bool>(type: "bit", nullable: false),
                    ConsultaFechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consultas", x => x.ConsultaId);
                    table.ForeignKey(
                        name: "FK_Consultas_Citas_CitaId",
                        column: x => x.CitaId,
                        principalTable: "Citas",
                        principalColumn: "CitaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Facturas",
                columns: table => new
                {
                    FacturaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FacturaNumero = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    PacienteId = table.Column<int>(type: "int", nullable: false),
                    CitaId = table.Column<int>(type: "int", nullable: true),
                    FacturaFecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FacturaSubtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FacturaDescuento = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FacturaImpuesto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FacturaTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FacturaEstado = table.Column<int>(type: "int", nullable: false),
                    FacturaFechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Facturas", x => x.FacturaId);
                    table.ForeignKey(
                        name: "FK_Facturas_Citas_CitaId",
                        column: x => x.CitaId,
                        principalTable: "Citas",
                        principalColumn: "CitaId");
                    table.ForeignKey(
                        name: "FK_Facturas_Pacientes_PacienteId",
                        column: x => x.PacienteId,
                        principalTable: "Pacientes",
                        principalColumn: "PacienteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConsultaDiagnosticos",
                columns: table => new
                {
                    ConsultaDiagnosticoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConsultaId = table.Column<int>(type: "int", nullable: false),
                    DiagnosticoId = table.Column<int>(type: "int", nullable: false),
                    EsPrincipal = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultaDiagnosticos", x => x.ConsultaDiagnosticoId);
                    table.ForeignKey(
                        name: "FK_ConsultaDiagnosticos_Consultas_ConsultaId",
                        column: x => x.ConsultaId,
                        principalTable: "Consultas",
                        principalColumn: "ConsultaId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConsultaDiagnosticos_Diagnosticos_DiagnosticoId",
                        column: x => x.DiagnosticoId,
                        principalTable: "Diagnosticos",
                        principalColumn: "DiagnosticoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrdenExamenes",
                columns: table => new
                {
                    OrdenExamenId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConsultaId = table.Column<int>(type: "int", nullable: false),
                    ExamenId = table.Column<int>(type: "int", nullable: false),
                    OrdenFecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrdenEstado = table.Column<int>(type: "int", nullable: false),
                    ResultadoTexto = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ResultadoFecha = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResultadoArchivoUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenExamenes", x => x.OrdenExamenId);
                    table.ForeignKey(
                        name: "FK_OrdenExamenes_Consultas_ConsultaId",
                        column: x => x.ConsultaId,
                        principalTable: "Consultas",
                        principalColumn: "ConsultaId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrdenExamenes_Examenes_ExamenId",
                        column: x => x.ExamenId,
                        principalTable: "Examenes",
                        principalColumn: "ExamenId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Recetas",
                columns: table => new
                {
                    RecetaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConsultaId = table.Column<int>(type: "int", nullable: false),
                    RecetaFechaEmision = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RecetaObservaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RecetaEstado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recetas", x => x.RecetaId);
                    table.ForeignKey(
                        name: "FK_Recetas_Consultas_ConsultaId",
                        column: x => x.ConsultaId,
                        principalTable: "Consultas",
                        principalColumn: "ConsultaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FacturaDetalle",
                columns: table => new
                {
                    FacturaDetalleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FacturaId = table.Column<int>(type: "int", nullable: false),
                    MedicamentoId = table.Column<int>(type: "int", nullable: true),
                    ExamenId = table.Column<int>(type: "int", nullable: true),
                    DetalleDescripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DetalleCantidad = table.Column<int>(type: "int", nullable: false),
                    DetallePrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DetalleDescuento = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DetalleTotalLinea = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacturaDetalle", x => x.FacturaDetalleId);
                    table.ForeignKey(
                        name: "FK_FacturaDetalle_Examenes_ExamenId",
                        column: x => x.ExamenId,
                        principalTable: "Examenes",
                        principalColumn: "ExamenId");
                    table.ForeignKey(
                        name: "FK_FacturaDetalle_Facturas_FacturaId",
                        column: x => x.FacturaId,
                        principalTable: "Facturas",
                        principalColumn: "FacturaId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FacturaDetalle_Medicamentos_MedicamentoId",
                        column: x => x.MedicamentoId,
                        principalTable: "Medicamentos",
                        principalColumn: "MedicamentoId");
                });

            migrationBuilder.CreateTable(
                name: "Pagos",
                columns: table => new
                {
                    PagoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FacturaId = table.Column<int>(type: "int", nullable: false),
                    PagoMonto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MetodoPagoId = table.Column<int>(type: "int", nullable: false),
                    PagoReferencia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PagoFecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PagoEstado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagos", x => x.PagoId);
                    table.ForeignKey(
                        name: "FK_Pagos_Facturas_FacturaId",
                        column: x => x.FacturaId,
                        principalTable: "Facturas",
                        principalColumn: "FacturaId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pagos_MetodoPagos_MetodoPagoId",
                        column: x => x.MetodoPagoId,
                        principalTable: "MetodoPagos",
                        principalColumn: "MetodoPagoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrdenExamenDetalles",
                columns: table => new
                {
                    OrdenExamenDetalleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrdenExamenId = table.Column<int>(type: "int", nullable: false),
                    ParametroNombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ResultadoValor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ResultadoUnidad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RangoReferencia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FueraDeRango = table.Column<bool>(type: "bit", nullable: true),
                    Observacion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenExamenDetalles", x => x.OrdenExamenDetalleId);
                    table.ForeignKey(
                        name: "FK_OrdenExamenDetalles_OrdenExamenes_OrdenExamenId",
                        column: x => x.OrdenExamenId,
                        principalTable: "OrdenExamenes",
                        principalColumn: "OrdenExamenId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecetaDetalles",
                columns: table => new
                {
                    RecetaDetalleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecetaId = table.Column<int>(type: "int", nullable: false),
                    MedicamentoId = table.Column<int>(type: "int", nullable: false),
                    Dosis = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Frecuencia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Duracion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Indicaciones = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecetaDetalles", x => x.RecetaDetalleId);
                    table.ForeignKey(
                        name: "FK_RecetaDetalles_Medicamentos_MedicamentoId",
                        column: x => x.MedicamentoId,
                        principalTable: "Medicamentos",
                        principalColumn: "MedicamentoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecetaDetalles_Recetas_RecetaId",
                        column: x => x.RecetaId,
                        principalTable: "Recetas",
                        principalColumn: "RecetaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Auditorias_UsuarioId",
                table: "Auditorias",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Citas_MedicoId",
                table: "Citas",
                column: "MedicoId");

            migrationBuilder.CreateIndex(
                name: "IX_Citas_PacienteId",
                table: "Citas",
                column: "PacienteId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultaDiagnosticos_ConsultaId",
                table: "ConsultaDiagnosticos",
                column: "ConsultaId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultaDiagnosticos_DiagnosticoId",
                table: "ConsultaDiagnosticos",
                column: "DiagnosticoId");

            migrationBuilder.CreateIndex(
                name: "IX_Consultas_CitaId",
                table: "Consultas",
                column: "CitaId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturaDetalle_ExamenId",
                table: "FacturaDetalle",
                column: "ExamenId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturaDetalle_FacturaId",
                table: "FacturaDetalle",
                column: "FacturaId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturaDetalle_MedicamentoId",
                table: "FacturaDetalle",
                column: "MedicamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_CitaId",
                table: "Facturas",
                column: "CitaId");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_PacienteId",
                table: "Facturas",
                column: "PacienteId");

            migrationBuilder.CreateIndex(
                name: "IX_HorarioMedicos_MedicoId",
                table: "HorarioMedicos",
                column: "MedicoId");

            migrationBuilder.CreateIndex(
                name: "IX_InventarioMovimientos_InventarioId",
                table: "InventarioMovimientos",
                column: "InventarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Inventarios_MedicamentoId",
                table: "Inventarios",
                column: "MedicamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Medicos_EspecialidadId",
                table: "Medicos",
                column: "EspecialidadId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenExamenDetalles_OrdenExamenId",
                table: "OrdenExamenDetalles",
                column: "OrdenExamenId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenExamenes_ConsultaId",
                table: "OrdenExamenes",
                column: "ConsultaId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenExamenes_ExamenId",
                table: "OrdenExamenes",
                column: "ExamenId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_FacturaId",
                table: "Pagos",
                column: "FacturaId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_MetodoPagoId",
                table: "Pagos",
                column: "MetodoPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_RecetaDetalles_MedicamentoId",
                table: "RecetaDetalles",
                column: "MedicamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_RecetaDetalles_RecetaId",
                table: "RecetaDetalles",
                column: "RecetaId");

            migrationBuilder.CreateIndex(
                name: "IX_Recetas_ConsultaId",
                table: "Recetas",
                column: "ConsultaId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioRoles_RolId",
                table: "UsuarioRoles",
                column: "RolId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioRoles_UsuarioId",
                table: "UsuarioRoles",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Auditorias");

            migrationBuilder.DropTable(
                name: "ConsultaDiagnosticos");

            migrationBuilder.DropTable(
                name: "FacturaDetalle");

            migrationBuilder.DropTable(
                name: "HorarioMedicos");

            migrationBuilder.DropTable(
                name: "InventarioMovimientos");

            migrationBuilder.DropTable(
                name: "OrdenExamenDetalles");

            migrationBuilder.DropTable(
                name: "Pagos");

            migrationBuilder.DropTable(
                name: "RecetaDetalles");

            migrationBuilder.DropTable(
                name: "UsuarioRoles");

            migrationBuilder.DropTable(
                name: "Diagnosticos");

            migrationBuilder.DropTable(
                name: "Inventarios");

            migrationBuilder.DropTable(
                name: "OrdenExamenes");

            migrationBuilder.DropTable(
                name: "Facturas");

            migrationBuilder.DropTable(
                name: "MetodoPagos");

            migrationBuilder.DropTable(
                name: "Recetas");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Medicamentos");

            migrationBuilder.DropTable(
                name: "Examenes");

            migrationBuilder.DropTable(
                name: "Consultas");

            migrationBuilder.DropTable(
                name: "Citas");

            migrationBuilder.DropTable(
                name: "Medicos");

            migrationBuilder.DropTable(
                name: "Pacientes");

            migrationBuilder.DropTable(
                name: "Especialidades");
        }
    }
}
