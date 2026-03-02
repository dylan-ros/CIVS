using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CIVS.Models
{
    public class UsuarioRol
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UsuarioRolId { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [ForeignKey(nameof(UsuarioId))]
        public Usuario Usuario { get; set; } = null!;

        [Required]
        public int RolId { get; set; }

        [ForeignKey(nameof(RolId))]
        public Rol Rol { get; set; } = null!;

        public DateTime UsuarioRolFechaRegistro { get; set; } = DateTime.UtcNow;

        public bool UsuarioRolEstado { get; set; } = true;
    }
}
