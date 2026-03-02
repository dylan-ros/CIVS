using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CIVS.Models
{
    public class MetodoPago
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MetodoPagoId { get; set; }

        [Required, StringLength(50)]
        public string MetodoPagoNombre { get; set; } = string.Empty;

        [StringLength(150)]
        public string? MetodoPagoDescripcion { get; set; }

        public bool MetodoPagoEstado { get; set; } = true;

        public DateTime MetodoPagoFechaRegistro { get; set; } = DateTime.UtcNow;
    }
}
