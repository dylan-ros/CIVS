using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CIVS.Models
{
    public class Consulta
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        public int ConsultaId { get; set; }

        [Required]
        public int CitaId { get; set; }

        [ForeignKey(nameof(CitaId))]
        public Cita Cita { get; set; } = null!;

        [Required, StringLength(200)]
        public string ConsultaSignosVitales { get; set; } = string.Empty;

        [Required, StringLength(1000)]
        public string ConsultaNotasClinicas { get; set; } = string.Empty;

        [Required, StringLength(1000)]
        public string ConsultaPlanTratamiento { get; set; } = string.Empty;

        public bool ConsultaEstado { get; set; } = true;

        public DateTime ConsultaFechaRegistro { get; set; } = DateTime.UtcNow;
    }
}
