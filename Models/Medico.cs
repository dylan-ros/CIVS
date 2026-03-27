using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CIVS.Models
{
    public class Medico
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MedicoId { get; set; }

        // FK Especialidad
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una especialidad.")]
        public int EspecialidadId { get; set; }

        [ForeignKey(nameof(EspecialidadId))]
        public Especialidad? Especialidad { get; set; }

        [Required, StringLength(50)]
        public string MedicoNombres { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string MedicoApellidos { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string MedicoColegiado { get; set; } = string.Empty;

        [Required, StringLength(18)]
        public string MedicoTelefono { get; set; } = string.Empty;

        [StringLength(120)]
        public string? MedicoEmail { get; set; }

        public bool MedicoEstado { get; set; } = true;

        public DateTime MedicoFechaRegistro { get; set; } = DateTime.UtcNow;





    }
}
