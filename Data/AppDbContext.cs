using CIVS.Models;
using Microsoft.EntityFrameworkCore;

namespace CIVS.Data
{

    // PARA AGREGAR MIGRACIONES DE LAS TABLAS A LA BASE DE DATOS:

    // dotnet ef migrations add Add"Clase"
    // dotnet ef database update
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Paciente> Pacientes { get; set; }
        public DbSet<Especialidad> Especialidades { get; set; }
        public DbSet<Medico> Medicos { get; set; }
        public DbSet<HorarioMedico> HorarioMedicos { get; set; }
        public DbSet<Cita> Citas { get; set; }
        public DbSet<Consulta> Consultas { get; set; }
        public DbSet<Diagnostico> Diagnosticos { get; set; }
        public DbSet<ConsultaDiagnostico> ConsultaDiagnosticos { get; set; }
        public DbSet<Receta> Recetas { get; set; }
        public DbSet<RecetaDetalle> RecetaDetalles { get; set; }
        public DbSet<Medicamento> Medicamentos { get; set; }
        public DbSet<Examen> Examenes { get; set; }
        public DbSet<OrdenExamen> OrdenExamenes { get; set; }
        public DbSet<OrdenExamenDetalle> OrdenExamenDetalles { get; set; }
        public DbSet<Inventario> Inventarios { get; set; }
        public DbSet<InventarioMovimiento> InventarioMovimientos { get; set; }
        public DbSet<Factura> Facturas { get; set; }
        public DbSet<FacturaDetalle> FacturaDetalle { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<MetodoPago> MetodoPagos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Auditoria> Auditorias { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<UsuarioRol> UsuarioRoles { get; set; }









    }
}