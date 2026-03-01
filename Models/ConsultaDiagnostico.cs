using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CIVS.Models
{
    public class ConsultaDiagnostico
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ConsultaDiagnosticoId { get; set; }

        [Required]
        public int ConsultaId { get; set; }

        [ForeignKey(nameof(ConsultaId))]
        public Consulta Consulta { get; set; } = null!;

        [Required]
        public int DiagnosticoId { get; set; }

        [ForeignKey(nameof(DiagnosticoId))]
        public Diagnostico Diagnostico { get; set; } = null!;

        // opcional: principal/secundario
        public bool EsPrincipal { get; set; } = false;
    }
}
