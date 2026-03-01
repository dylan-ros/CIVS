using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CIVS.Models
{
    public class RecetaDetalle
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecetaDetalleId { get; set; }

        [Required]
        public int RecetaId { get; set; }

        [ForeignKey(nameof(RecetaId))]
        public Receta Receta { get; set; } = null!;

        // FK a catálogo Medicamentos
        [Required]
        public int MedicamentoId { get; set; }

        [ForeignKey(nameof(MedicamentoId))]
        public Medicamento Medicamento { get; set; } = null!;

        [StringLength(100)]
        public string? Dosis { get; set; }  // ej: "500 mg"

        [StringLength(100)]
        public string? Frecuencia { get; set; } // ej: "Cada 8 horas"

        [StringLength(100)]
        public string? Duracion { get; set; } // ej: "7 días"

        [StringLength(300)]
        public string? Indicaciones { get; set; } // ej: "Con comida"
    }
}
