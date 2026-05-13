using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CIVS.Models
{
    public enum EstadoFactura
    {
        Emitida = 1,
        Pagada = 2,
        Anulada = 3
    }

    public class Factura
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FacturaId { get; set; }

        // Opcional: correlativo visible
        [StringLength(30)]
        public string? FacturaNumero { get; set; }

        [Required]
        public int PacienteId { get; set; }

        [ForeignKey(nameof(PacienteId))]
        public Paciente Paciente { get; set; } = null!;

        // Opcional: si factura está asociada a una cita
        public int? CitaId { get; set; }

        [ForeignKey(nameof(CitaId))]
        public Cita? Cita { get; set; }

        public DateTime FacturaFecha { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal FacturaSubtotal { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal FacturaDescuento { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal FacturaImpuesto { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal FacturaTotal { get; set; } = 0;

        public EstadoFactura FacturaEstado { get; set; } = EstadoFactura.Emitida;

        public DateTime FacturaFechaRegistro { get; set; } = DateTime.UtcNow;

        public ICollection<FacturaDetalle> Detalles { get; set; } = new List<FacturaDetalle>();
        public ICollection<Pago> Pagos { get; set; } = new List<Pago>();

    }
}
