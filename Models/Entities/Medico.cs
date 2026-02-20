using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CIVS.Models.Entities
{
    public class Medico
    {

        /* ID DEL MEDICO */
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MedicoId { get; set; }

        /* Propiedad especialidad */
        [Required]
        public int EspecialidadId { get; set; }

        /*Refencia FK Especialidad Id*/
        [ForeignKey(nameof(EspecialidadId))]
        public Especialidad Especialidad { get; set; } = null!;

        /* NOMBRES MEDICO */
        [Required, StringLength(120)]
        public string NombreMedico { get; set; } = string.Empty;

        /* APELLIDOS MEDICO */
        [Required, StringLength(120)]
        public string ApellidoMedico { get; set; } = string.Empty;

        /* COLEGIADO DEL MEDICO */
        [Required, StringLength(50)]
        public string Colegiado {  get; set; } = string.Empty;

        /* TELEFONO MEDICO */
        [Required, StringLength(20)]
        public string TelefonoMedico { get; set; } = string.Empty;

        [Required, StringLength(1)]
        public string GeneroPaciente { get; set; } = string.Empty;

        public bool EstadoMedico { get; set; } = true;

        public DateTime FechaRegistroMedico { get; set; } = DateTime.UtcNow;

    }
}
