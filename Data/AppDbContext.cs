using CIVS.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CIVS.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Paciente> Pacientes => Set<Paciente>(); /* PACIENTE */
        public DbSet<Medico> Medicos => Set<Medico>(); /* MEDICO */
        public DbSet<Especialidad> Especialidades => Set<Especialidad>(); /* ESPECIALIDAD */
        public DbSet<Cita> Citas => Set<Cita>(); /* CITAS */
        public DbSet<Consulta> Consultas => Set<Consulta>(); /* CONSULTAS */
        public DbSet<HorarioMedico> HorariosMedicos => Set<HorarioMedico>(); /* HorarioMedico */
        public DbSet<Diagnostico> Diagnosticos => Set<Diagnostico>();
        public DbSet<Receta> Recetas => Set<Receta>();

    }
}
