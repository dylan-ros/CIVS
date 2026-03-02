using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CIVS.Models
{
    public enum TipoInventarioMovimiento
    {
        Entrada = 1,
        Salida = 2,
        Ajuste = 3
    }

    public class InventarioMovimiento
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InventarioMovimientoId { get; set; }

        [Required]
        public int InventarioId { get; set; }

        [ForeignKey(nameof(InventarioId))]
        public Inventario Inventario { get; set; } = null!;

        [Required]
        public TipoInventarioMovimiento MovimientoTipo { get; set; }

        [Required]
        public int MovimientoCantidad { get; set; }

        [StringLength(200)]
        public string? MovimientoMotivo { get; set; }

        public DateTime MovimientoFecha { get; set; } = DateTime.UtcNow;

        public bool MovimientoEstado { get; set; } = true;

    }
}
