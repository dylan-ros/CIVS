using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CIVS.Models
{
    public class Diagnostico
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DiagnosticoId { get; set; }

        // Si usas CIE-10: "J00", "E11", etc se puede poner aquí
        [Required, StringLength(200)]
        public string DiagnosticoCodigo { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string DiagnosticoNombre { get; set; } = string.Empty;

        public bool DiagnosticoEstado { get; set; } = true;

    }
}
