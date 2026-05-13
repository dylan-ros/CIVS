using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CIVS.Models
{
    public class Usuario
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UsuarioId { get; set; }

        [Required, StringLength(50)]
        public string UsuarioUsername { get; set; } = string.Empty;

        [Required, StringLength(120), EmailAddress]
        public string UsuarioEmail { get; set; } = string.Empty;

        [Required, StringLength(255)]
        public string UsuarioPasswordHash { get; set; } = string.Empty;

        public bool UsuarioEstado { get; set; } = true;

        public DateTime UsuarioFechaRegistro { get; set; } = DateTime.UtcNow;

        [StringLength(80)]
        public string? UsuarioNombres { get; set; }

        [StringLength(80)]
        public string? UsuarioApellidos { get; set; }

        // ── Control de sesion unica ─---
        [StringLength(100)]
        public string? SessionToken { get; set; }

        public DateTime? SessionTokenExpiry { get; set; }

        // Navegación a tabla puente
        public ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();
    }
}
