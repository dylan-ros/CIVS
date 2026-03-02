using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CIVS.Models
{
    public class Inventario
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InventarioId { get; set; }

        [Required]
        public int MedicamentoId { get; set; }

        [ForeignKey(nameof(MedicamentoId))]
        public Medicamento Medicamento { get; set; } = null!;

        // Existencias
        [Required]
        public int StockActual { get; set; } = 0;
        public int StockMinimo { get; set; } = 0;

        // Opcional: si manejas lotes/fechas de vencimiento por medicamento (más pro se hace con otra tabla)
        public DateTime? FechaVencimiento { get; set; }
        public bool InventarioEstado { get; set; } = true;
        public DateTime InventarioFechaRegistro { get; set; } = DateTime.UtcNow;
    }
}
