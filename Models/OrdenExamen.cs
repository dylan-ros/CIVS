using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CIVS.Models
{

    public enum EstadoOrdenExamen
    {
        Solicitado = 1,
        Tomado = 2,
        Procesado = 3,
        Entregado = 4,
        Cancelado = 5
    }

    public class OrdenExamen
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrdenExamenId { get; set; }

        [Required]
        public int ConsultaId { get; set; }

        [ForeignKey(nameof(ConsultaId))]
        public Consulta Consulta { get; set; } = null!;

        [Required]
        public int ExamenId { get; set; }

        [ForeignKey(nameof(ExamenId))]
        public Examen Examen { get; set; } = null!;

        public DateTime OrdenFecha { get; set; } = DateTime.UtcNow;

        [Required]
        public EstadoOrdenExamen OrdenEstado { get; set; } = EstadoOrdenExamen.Solicitado;

        // Resultado (opcional)
        [StringLength(2000)]
        public string? ResultadoTexto { get; set; }

        public DateTime? ResultadoFecha { get; set; }

        [StringLength(200)]
        public string? ResultadoArchivoUrl { get; set; } // si subís PDF/imagen

        // Detalles (parámetros/resultados por item)
        public ICollection<OrdenExamenDetalle> OrdenDetalles { get; set; } = new List<OrdenExamenDetalle>();

    }
}
