using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CIVS.Models.Entities
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

        public DateTime FechaEmisionReceta { get; set; } = DateTime.UtcNow;

        [Required, StringLength(500)]
        public string Observaciones { get; set; } = string.Empty;
        public bool EstadoReceta { get; set; } = true;
    }
}
