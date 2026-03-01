using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CIVS.Models
{
    public class HorarioMedico
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        [Required]
        public int HorarioMedicoId { get; set; }

        public int MedicoId { get; set; }

        [ForeignKey(nameof(MedicoId))]
        public Medico Medico { get; set; } = null!;

        [Required]
        public DateTime MedicoHorarioInicio { get; set; }

        [Required]
        public DateTime MedicoHorarioFin { get; set; }
        public bool MedicoHorarioDisponible { get; set; } = true;

        [StringLength(200)] 
        public string? HorarioNota { get; set; }

        public DateTime HorarioFechaRegistro { get; set; } = DateTime.UtcNow;



    }
}
