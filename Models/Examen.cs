using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CIVS.Models
{
    public class Examen
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ExamenId { get; set; }

        [Required, StringLength(100)]
        public string ExamenNombre { get; set; } = string.Empty;

        [StringLength(500)]
        public string? ExamenDescripcion { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal? ExamenPrecio {  get; set; }

        public bool ExamenEstado {  get; set; }

        public DateTime ExamenFechaRegistro { get; set; } = DateTime.UtcNow;

    }
}
