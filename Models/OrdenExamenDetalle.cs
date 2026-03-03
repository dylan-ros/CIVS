using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CIVS.Models
{
    public class OrdenExamenDetalle
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrdenExamenDetalleId { get; set; }

        [Required]
        public int OrdenExamenId { get; set; }

        [ForeignKey(nameof(OrdenExamenId))]
        public OrdenExamen OrdenExamen { get; set; } = null!;

        // Nombre del parámetro / prueba (ej: "Glucosa", "Hemoglobina", "Leucocitos")
        [Required, StringLength(150)]
        public string ParametroNombre { get; set; } = string.Empty;

        // Resultado (texto porque a veces es "Negativo/Positivo", "Normal", etc.)
        [StringLength(100)]
        public string? ResultadoValor { get; set; }

        [StringLength(50)]
        public string? ResultadoUnidad { get; set; } // ej: "mg/dL", "g/dL"

        [StringLength(100)]
        public string? RangoReferencia { get; set; } // ej: "70-110"

        // opcional: flag para fuera de rango
        public bool? FueraDeRango { get; set; }

        // opcional: observación por parámetro
        [StringLength(300)]
        public string? Observacion { get; set; }
    }
}
