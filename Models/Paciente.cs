using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CIVS.Models
{
    public class Paciente
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PacienteId { get; set; }

        [Required, StringLength(13)]
        public string PacienteDPI { get; set; } = string.Empty;        

        [Required, StringLength(50)]
        public string PacienteNombres { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string PacienteApellido { get; set; } = string.Empty;

        [Required, StringLength(18)]
        public string PacienteTelefono {  get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(120)]
        public string PacienteCorreo { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string PacienteDireccion { get; set; } = string.Empty;

        [Required]
        public DateTime PacienteNacimiento { get; set; }

        [StringLength(12)]
        public string? PacienteEstadoCivil { get; set; }

        public bool PacienteEstado { get; set; } = true;

        public DateTime PacienteFechaRegistro { get; set; } = DateTime.UtcNow;



    }
}
