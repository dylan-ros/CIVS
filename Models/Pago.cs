using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CIVS.Models
{
    public class Pago
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PagoId { get; set; }

        [Required]
        public int FacturaId { get; set; }

        [ForeignKey(nameof(FacturaId))]
        public Factura Factura { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal PagoMonto { get; set; }
        
        [Required]
        public int MetodoPagoId { get; set; }

        [ForeignKey(nameof(MetodoPagoId))]
        public MetodoPago MetodoPago { get; set; } = null!;

        [StringLength(100)]
        public string? PagoReferencia { get; set; }

        public DateTime PagoFecha { get; set; } = DateTime.UtcNow;

        public bool PagoEstado { get; set; } = true;
    }
}
