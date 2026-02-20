using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CIVS.Models.Entities
{
    public class Diagnostico
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DiagnosticoId { get; set; }

        [Required]
        public int ConsultaId { get; set; }

        [ForeignKey(nameof(ConsultaId))]
        public Consulta Consulta { get; set; } = null!;
    }
}
