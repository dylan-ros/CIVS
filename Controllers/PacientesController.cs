using CIVS.Data;
using CIVS.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CIVS.Controllers
{
    [Authorize]
    public class PacienteController : Controller
    {
        private readonly AppDbContext _context;

        public PacienteController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Paciente (Consulta de pacientes + buscador)
        public async Task<IActionResult> Index(string? q)
        {
            var query = _context.Pacientes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();

                query = query.Where(p =>
                    p.PacienteDPI.Contains(q) ||
                    p.PacienteNombres.Contains(q) ||
                    p.PacienteApellido.Contains(q) ||
                    (p.PacienteNombres + " " + p.PacienteApellido).Contains(q) || 
                    p.PacienteTelefono.Contains(q) ||
                    (p.PacienteCorreo != null && p.PacienteCorreo.Contains(q)) ||
                    (p.PacienteDireccion != null && p.PacienteDireccion.Contains(q)));
            }

            var pacientes = await query
                .OrderByDescending(p => p.PacienteFechaRegistro)
                .ToListAsync();

            ViewBag.Q = q;
            return View(pacientes); // Views/Paciente/Index.cshtml
        }

        // GET: Paciente/CrearPaciente
        public IActionResult CrearPaciente()
        {
            return View(); // Views/Paciente/CrearPaciente.cshtml
        }

        // POST: Paciente/CrearPaciente (crear)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearPaciente(Paciente paciente)
        {
            if (!ModelState.IsValid)
                return View(paciente);

            paciente.PacienteEstado = true;

            _context.Pacientes.Add(paciente);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Paciente creado exitosamente.";
            return RedirectToAction(nameof(CrearPaciente));
        }

        // GET: Paciente/EditarPaciente/5
        public async Task<IActionResult> EditarPaciente(int id)
        {
            var paciente = await _context.Pacientes.FindAsync(id);

            if (paciente == null)
                return NotFound();

            return View(paciente); // Views/Paciente/EditarPaciente.cshtml
        }

        // POST: Paciente/EditarPaciente/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPaciente(int id, Paciente datos)
        {
            var paciente = await _context.Pacientes.FindAsync(id);

            if (paciente == null)
                return NotFound();

            if (!ModelState.IsValid)
                return View(datos);

            paciente.PacienteDPI = datos.PacienteDPI;
            paciente.PacienteNombres = datos.PacienteNombres;
            paciente.PacienteApellido = datos.PacienteApellido;
            paciente.PacienteTelefono = datos.PacienteTelefono;
            paciente.PacienteCorreo = datos.PacienteCorreo;
            paciente.PacienteDireccion = datos.PacienteDireccion;
            paciente.PacienteNacimiento = datos.PacienteNacimiento;

            await RegistrarAuditoriaAsync(
                "UPDATE",
                "Paciente",
                paciente.PacienteId.ToString(),
                $"Editó los datos del paciente: {paciente.PacienteNombres} {paciente.PacienteApellido}."
            );

            await _context.SaveChangesAsync();

            TempData["Success"] = "Paciente actualizado exitosamente.";
            return RedirectToAction(nameof(Index), new { q = paciente.PacienteDPI });
        }

        // POST: Paciente/ActivarDesactivar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivarDesactivar(int id)
        {
            var paciente = await _context.Pacientes.FindAsync(id);

            if (paciente == null)
                return NotFound();

            paciente.PacienteEstado = !paciente.PacienteEstado;

            string accion = paciente.PacienteEstado ? "ACTIVATE" : "DEACTIVATE";
            string estadoTexto = paciente.PacienteEstado ? "activó" : "desactivó";

            await RegistrarAuditoriaAsync(
                accion,
                "Paciente",
                paciente.PacienteId.ToString(),
                $"El usuario {estadoTexto} al paciente: {paciente.PacienteNombres} {paciente.PacienteApellido}."
            );

            await _context.SaveChangesAsync();

            TempData["Success"] = paciente.PacienteEstado
                ? "Paciente activado exitosamente."
                : "Paciente desactivado exitosamente.";

            return RedirectToAction(nameof(Index), new { q = paciente.PacienteDPI });
        }

        // ── Helpers de auditoría ────────────────────────────────────────────────
        private async Task RegistrarAuditoriaAsync(string accion, string entidad, string entidadId, string descripcion)
        {
            var usuarioId = await ObtenerUsuarioActualIdAsync();

            _context.Auditorias.Add(new Auditoria
            {
                UsuarioId = usuarioId,
                AuditoriaAccion = accion,
                AuditoriaEntidad = entidad,
                AuditoriaEntidadId = entidadId,
                AuditoriaDescripcion = descripcion,
                AuditoriaFecha = DateTime.Now
            });
        }

        private async Task<int?> ObtenerUsuarioActualIdAsync()
        {
            var username = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(username))
            {
                username =
                    User.FindFirst(ClaimTypes.Name)?.Value ??
                    User.FindFirst(ClaimTypes.Email)?.Value ??
                    User.FindFirst("UsuarioUsername")?.Value;
            }

            if (string.IsNullOrWhiteSpace(username))
                return null;

            return await _context.Usuarios
                .Where(u => u.UsuarioUsername == username || u.UsuarioEmail == username)
                .Select(u => (int?)u.UsuarioId)
                .FirstOrDefaultAsync();
        }






    }
}