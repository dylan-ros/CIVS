using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CIVS.Models
{
    public class Auditoria
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AuditoriaId { get; set; }

        // Quién realizó la acción (si ya tienes Usuario)
        public int? UsuarioId { get; set; }

        [ForeignKey(nameof(UsuarioId))]
        public Usuario? Usuario { get; set; }

        // Qué acción fue (CREATE/UPDATE/DELETE/LOGIN, etc.)
        [Required, StringLength(20)]
        public string AuditoriaAccion { get; set; } = string.Empty;

        // En qué entidad ocurrió (Paciente, Cita, Factura, etc.)
        [Required, StringLength(80)]
        public string AuditoriaEntidad { get; set; } = string.Empty;

        // ID del registro afectado (en texto para soportar int, guid, etc.)
        [StringLength(50)]
        public string? AuditoriaEntidadId { get; set; }

        // Datos extra / detalle del cambio
        [StringLength(1000)]
        public string? AuditoriaDescripcion { get; set; }

        public DateTime AuditoriaFecha { get; set; } = DateTime.UtcNow;

        public bool AuditoriaEstado { get; set; } = true;
    }
}
