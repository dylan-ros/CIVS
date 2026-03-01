using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CIVS.Models
{
    public class Receta
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecetaId { get; set; }

        [Required]
        public int ConsultaId { get; set; }

        [ForeignKey(nameof(ConsultaId))]
        public Consulta Consulta { get; set; } = null!;
        public DateTime RecetaFechaEmision { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string? RecetaObservaciones { get; set; }
        public bool RecetaEstado { get; set; } = true;


    }
}
