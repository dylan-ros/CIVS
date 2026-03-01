using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CIVS.Models
{
    public class Medicamento
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MedicamentoId { get; set; }

        [Required, StringLength(200)]
        public string MedicamentoNombre { get; set; } = string.Empty;

        // Ej: "Tabletas", "Jarabe", "Crema", "Inyección"
        [StringLength(80)]
        public string? MedicamentoPresentacion { get; set; }

        // Ej: "500 mg", "5 mg/5 ml"
        [StringLength(80)]
        public string? MedicamentoConcentracion { get; set; }

        // Ej: "Caja x 10", "Frasco 120ml"
        [StringLength(80)]
        public string? MedicamentoUnidad { get; set; }

        // Opcional: para farmacia (si después manejas precios)
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MedicamentoPrecio { get; set; }

        public bool MedicamentoEstado { get; set; } = true;

        public DateTime MedicamentoFechaRegistro { get; set; } = DateTime.UtcNow;

    }
}
