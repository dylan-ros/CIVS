using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CIVS.Models
{

    public enum EstadoCita
    {
        programada = 1, 
        confirmada = 2,
        cancelada = 3, 
        atendida = 4, 
        noAsistio = 5
    }
    public class Cita
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CitaId { get; set; }

        [Required]
        public int PacienteId { get; set; }

        [ForeignKey(nameof(PacienteId))]
        public Paciente Paciente { get; set; } = null!;

        [Required]
        public int MedicoId { get; set; }

        [ForeignKey(nameof(MedicoId))]
        public Medico Medico { get; set; } = null!;

        [Required]
        public DateTime CitaFechaInicio { get; set; }

        [Required]
        public DateTime CitaFechafin { get; set; }

        [Required, StringLength(500)]
        public string CitaMotivo { get; set; } = string.Empty;

        [Required]
        public EstadoCita EstadoCita { get; set; } = EstadoCita.programada;

        public DateTime CitaFechaCreada { get; set; } = DateTime.UtcNow;

    }
}
