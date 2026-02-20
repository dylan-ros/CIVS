using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CIVS.Models.Entities
{
    public class Paciente
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PacienteId { get; set; }

        [Required, StringLength(13)]
        public string DPIPaciente { get; set; } = string.Empty; 

        [Required, StringLength(20)]
        public string NombresPaciente { get; set; } = string.Empty; /*Usado para que tengan un valor por defecto y evitar errores por null*/

        [Required, StringLength(20)]
        public string ApellidosPaciente { get; set; } = string.Empty; /*Usado para que tengan un valor por defecto y evitar errores por null*/

        [Required, DataType(DataType.Date)]
        public DateTime FechaNacimientoPaciente { get; set; }

        [Required, StringLength(1)]
        public string GeneroPaciente { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string TelefonoPaciente { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(120)]
        public string? EmailPaciente { get; set; }

        [Required, StringLength(120)]
        public string DireccionPaciente { get; set; } = string.Empty;

        public bool EstadoPaciente { get; set; } = true;

        public DateTime FechaRegistroPaciente { get; set; } = DateTime.UtcNow;


    }
}
