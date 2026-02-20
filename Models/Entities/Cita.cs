using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CIVS.Models.Entities
{
    public class Cita
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CitaId { get; set; }

        /* Propiedad Paciente */
        [Required]
        public int PacienteId { get; set; }

        /*Refencia FK Paciente Id*/
        [ForeignKey(nameof(PacienteId))]
        public Paciente Paciente { get; set; } = null!;

        /* Propiedad Medico */
        [Required]
        public int MedicoId { get; set; }

        /*Refencia FK Medico Id*/
        [ForeignKey(nameof(MedicoId))]
        public Medico Medico { get; set;} = null!;  

        [Required]
        public DateTime FechaHoraInicio { get; set; }

        [Required]
        public DateTime FechaHoraFin { get; set; }

        [Required, StringLength(200)]
        public string Motivo {  get; set; } = string.Empty;

        public bool EstadoCita { get; set; } = true;

        public DateTime FechaRegistroCita { get; set; } = DateTime.UtcNow;




    }
}
