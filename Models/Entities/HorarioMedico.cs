using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CIVS.Models.Entities
{
    public class HorarioMedico
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int HorarioMedicoId { get; set; }

        [Required]
        public int MedicoId { get; set; }

        [ForeignKey(nameof(MedicoId))]
        public Medico Medico { get; set; } = null!;

        [Required]
        public DateTime FechaHoraInicio { get; set; }

        [Required]
        public DateTime FechaHoraFin { get; set; }

        public bool Activo { get; set; } = true;
    }
}
