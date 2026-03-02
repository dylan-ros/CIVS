using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CIVS.Models
{
    public class Rol
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RolId { get; set; }

        [Required, StringLength(50)]
        public string RolNombre { get; set; } = string.Empty;

        [StringLength(150)]
        public string? RolDescripcion { get; set; }

        public bool RolEstado { get; set; } = true;

        public DateTime RolFechaRegistro { get; set; } = DateTime.UtcNow;

        // Navegación a tabla puente
        public ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();
    }
}
