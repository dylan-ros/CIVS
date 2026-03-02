using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CIVS.Models
{
    public class FacturaDetalle
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FacturaDetalleId { get; set; }

        [Required]
        public int FacturaId { get; set; }

        [ForeignKey(nameof(FacturaId))]
        public Factura Factura { get; set; } = null!;

        public int? MedicamentoId { get; set; }

        [ForeignKey(nameof(MedicamentoId))]
        public Medicamento? Medicamento { get; set; }

        public int? ExamenId { get; set; }

        [ForeignKey(nameof(ExamenId))]
        public Examen? Examen { get; set; }

        [Required, StringLength(200)]
        public string DetalleDescripcion { get; set; } = string.Empty;

        [Required]
        public int DetalleCantidad { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DetallePrecioUnitario { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DetalleDescuento { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DetalleTotalLinea { get; set; } = 0;
    }
}
