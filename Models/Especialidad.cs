using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CIVS.Models
{
    public class Especialidad
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EspecialidadId { get; set; }

        [Required, StringLength(120)]
        public string EspecialidadNombre { get; set; } = string.Empty;

        public bool EspecialidadEstado { get; set; } = true;


    }
}
