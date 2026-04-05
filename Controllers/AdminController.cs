using CIVS.Data;
using CIVS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace CIVS.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // ── GET: /Admin (Panel principal) ────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            ViewBag.TotalUsuarios = await _context.Usuarios.CountAsync();
            ViewBag.TotalActivos = await _context.Usuarios.CountAsync(u => u.UsuarioEstado);
            ViewBag.TotalRoles = await _context.Roles.CountAsync();
            ViewBag.TotalAuditorias = await _context.Auditorias.CountAsync();
            return View();
        }

        // ── GET: /Admin/Usuarios ─────────────────────────────────────────────
        public async Task<IActionResult> Usuarios(string? q)
        {
            var query = _context.Usuarios
                .Include(u => u.UsuarioRoles)
                    .ThenInclude(ur => ur.Rol)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(u =>
                    u.UsuarioUsername.Contains(q) ||
                    u.UsuarioEmail.Contains(q) ||
                    (u.UsuarioNombres != null && u.UsuarioNombres.Contains(q)) ||
                    (u.UsuarioApellidos != null && u.UsuarioApellidos.Contains(q)));
            }

            var usuarios = await query
                .OrderByDescending(u => u.UsuarioFechaRegistro)
                .ToListAsync();

            ViewBag.Q = q;
            return View(usuarios);
        }

        // ── GET: /Admin/CrearUsuario ─────────────────────────────────────────
        public async Task<IActionResult> CrearUsuario()
        {
            await CargarRoles();
            await CargarEspecialidades();
            return View();
        }

        // ── POST: /Admin/CrearUsuario ────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearUsuario(
    string UsuarioUsername,
    string UsuarioEmail,
    string UsuarioPassword,
    string? UsuarioNombres,
    string? UsuarioApellidos,
    int RolId,
    // Datos extra si es Médico
    int? EspecialidadId,
    string? MedicoColegiado,
    string? MedicoTelefono)
        {
            if (await _context.Usuarios.AnyAsync(u => u.UsuarioUsername == UsuarioUsername))
                ModelState.AddModelError("UsuarioUsername",
                    "Ya existe un usuario con ese nombre de usuario.");

            if (await _context.Usuarios.AnyAsync(u => u.UsuarioEmail == UsuarioEmail))
                ModelState.AddModelError("UsuarioEmail",
                    "Ya existe un usuario registrado con ese correo.");

            if (string.IsNullOrWhiteSpace(UsuarioPassword) || UsuarioPassword.Length < 6)
                ModelState.AddModelError("UsuarioPassword",
                    "La contraseña debe tener al menos 6 caracteres.");

            // Si el rol es Médico, validar datos del médico
            var rolMedico = await _context.Roles
                .FirstOrDefaultAsync(r => r.RolNombre == "Medico");

            bool esMedico = rolMedico != null && RolId == rolMedico.RolId;

            if (esMedico)
            {
                if (!EspecialidadId.HasValue || EspecialidadId == 0)
                    ModelState.AddModelError("EspecialidadId",
                        "Debe seleccionar una especialidad para el médico.");
                if (string.IsNullOrWhiteSpace(MedicoColegiado))
                    ModelState.AddModelError("MedicoColegiado",
                        "El número de colegiado es obligatorio.");
                if (string.IsNullOrWhiteSpace(MedicoTelefono))
                    ModelState.AddModelError("MedicoTelefono",
                        "El teléfono del médico es obligatorio.");
            }

            if (!ModelState.IsValid)
            {
                await CargarRoles(RolId);
                await CargarEspecialidades();
                return View();
            }

            // Crear usuario
            var usuario = new Usuario
            {
                UsuarioUsername = UsuarioUsername,
                UsuarioEmail = UsuarioEmail,
                UsuarioPasswordHash = GenerarHash(UsuarioPassword),
                UsuarioNombres = UsuarioNombres,
                UsuarioApellidos = UsuarioApellidos,
                UsuarioEstado = true,
                UsuarioFechaRegistro = DateTime.UtcNow
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            // Asignar rol
            if (RolId > 0)
            {
                _context.UsuarioRoles.Add(new UsuarioRol
                {
                    UsuarioId = usuario.UsuarioId,
                    RolId = RolId,
                    UsuarioRolFechaRegistro = DateTime.UtcNow,
                    UsuarioRolEstado = true
                });
                await _context.SaveChangesAsync();
            }

            // Si es médico → crear también el registro en Medicos
            if (esMedico)
            {
                var medico = new Medico
                {
                    EspecialidadId = EspecialidadId!.Value,
                    MedicoNombres = UsuarioNombres ?? "",
                    MedicoApellidos = UsuarioApellidos ?? "",
                    MedicoColegiado = MedicoColegiado!,
                    MedicoTelefono = MedicoTelefono!,
                    MedicoEmail = UsuarioEmail,
                    MedicoEstado = true,
                    MedicoFechaRegistro = DateTime.UtcNow
                };

                _context.Medicos.Add(medico);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = esMedico
                ? $"Médico '{usuario.UsuarioUsername}' creado con usuario y ficha médica."
                : $"Usuario '{usuario.UsuarioUsername}' creado exitosamente.";

            return RedirectToAction(nameof(CrearUsuario));
        }

        // ── POST: /Admin/ActivarDesactivar ───────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivarDesactivar(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            usuario.UsuarioEstado = !usuario.UsuarioEstado;
            await _context.SaveChangesAsync();

            TempData["Success"] = usuario.UsuarioEstado
                ? $"Usuario '{usuario.UsuarioUsername}' activado."
                : $"Usuario '{usuario.UsuarioUsername}' desactivado.";

            return RedirectToAction(nameof(Usuarios));
        }

        // ── GET: /Admin/Auditoria ────────────────────────────────────────────
        public async Task<IActionResult> Auditoria(string? q, string? accion, DateTime? desde, DateTime? hasta)
        {
            var query = _context.Auditorias
                .Include(a => a.Usuario)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(a =>
                    a.AuditoriaEntidad.Contains(q) ||
                    (a.AuditoriaDescripcion != null && a.AuditoriaDescripcion.Contains(q)) ||
                    (a.AuditoriaEntidadId != null && a.AuditoriaEntidadId.Contains(q)) ||
                    (a.Usuario != null && a.Usuario.UsuarioUsername.Contains(q)));
            }

            if (!string.IsNullOrWhiteSpace(accion))
                query = query.Where(a => a.AuditoriaAccion == accion);

            if (desde.HasValue)
                query = query.Where(a => a.AuditoriaFecha >= desde.Value);

            if (hasta.HasValue)
                query = query.Where(a => a.AuditoriaFecha <= hasta.Value.AddDays(1));

            var auditorias = await query
                .OrderByDescending(a => a.AuditoriaFecha)
                .Take(500)
                .ToListAsync();

            ViewBag.Q = q;
            ViewBag.Accion = accion;
            ViewBag.Desde = desde?.ToString("yyyy-MM-dd");
            ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd");
            ViewBag.Acciones = new SelectList(new[]
            {
                "LOGIN", "LOGOUT", "CREATE", "UPDATE", "DELETE"
            });

            return View(auditorias);
        }

        // ── Helper: hash SHA-256 ─────────────────────────────────────────────
        private static string GenerarHash(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes).ToLower();
        }

        // ── Helper: cargar roles para el select ──────────────────────────────
        private async Task CargarRoles(int? seleccionado = null)
        {
            var roles = await _context.Roles
                .Where(r => r.RolEstado)
                .OrderBy(r => r.RolNombre)
                .ToListAsync();

            ViewBag.Roles = new SelectList(
                roles,
                nameof(Rol.RolId),
                nameof(Rol.RolNombre),
                seleccionado);
        }

        private async Task CargarEspecialidades()
        {
            ViewBag.Especialidades = new SelectList(
                await _context.Especialidades
                    .Where(e => e.EspecialidadEstado)
                    .OrderBy(e => e.EspecialidadNombre)
                    .ToListAsync(),
                "EspecialidadId", "EspecialidadNombre");
        }

    }
}