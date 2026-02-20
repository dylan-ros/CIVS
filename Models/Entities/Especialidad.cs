using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace CIVS.Models.Entities
{
    public class Especialidad
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EspecialidadId { get; set; }

        [Required, StringLength(120)]
        public string NombreEspecialidad { get; set; } = string.Empty;

    }
}
