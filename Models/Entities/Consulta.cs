using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CIVS.Models.Entities
{
    public class Consulta
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ConsultaId { get; set; }

        [Required]
        public int CitaId { get; set; }

        [ForeignKey(nameof(CitaId))]
        public Cita Cita { get; set; } = null!;

        [Required]
        public DateTime? FechaHoraConsulta { get; set; } 

        [Required, StringLength(100)]
        public string SignosVitales {  get; set; } = string.Empty;

        [Required, StringLength(500)]
        public string NotasClinicas {  get; set; } = string.Empty;

        [Required, StringLength(500)]
        public string PlanTratamiento {  get; set; } = string.Empty;

        public bool EstadoConsulta { get; set; } = true;

    }
}
