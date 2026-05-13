using CIVS.Data;
using CIVS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CIVS.Controllers
{
    [Authorize(Roles = "Administrador,Recepcionista")]
    public class MedicoController : Controller
    {
        private readonly AppDbContext _context;

        public MedicoController(AppDbContext context)
        {
            _context = context;
        }

        // ── GET: /Medico ─────────────────────────────────────────────────────
        public async Task<IActionResult> Index(string? q)
        {
            var query = _context.Medicos
                .Include(m => m.Especialidad)
                .AsQueryable();

            if (string.IsNullOrWhiteSpace(q))
            {
                ViewBag.Q = q;
                return View(new List<Medico>());
            }

            q = q.Trim();

            query = query.Where(m =>
                m.MedicoNombres.Contains(q) ||
                m.MedicoApellidos.Contains(q) ||
                (m.MedicoNombres + " " + m.MedicoApellidos).Contains(q) ||
                m.MedicoColegiado.Contains(q) ||
                m.MedicoTelefono.Contains(q) ||
                (m.MedicoEmail != null && m.MedicoEmail.Contains(q)) ||
                (m.Especialidad != null && m.Especialidad.EspecialidadNombre.Contains(q))
            );

            var medicos = await query
                .OrderByDescending(m => m.MedicoFechaRegistro)
                .ToListAsync();

            ViewBag.Q = q;
            return View(medicos);
        }

        // ── GET: /Medico/CrearMedico ─────────────────────────────────────────
        public async Task<IActionResult> CrearMedico()
        {
            await CargarEspecialidades();
            return View();
        }

        // ── POST: /Medico/CrearMedico ────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearMedico(Medico medico)
        {
            medico.MedicoEstado = true;
            medico.MedicoFechaRegistro = DateTime.UtcNow;

            bool colegiadoExiste = await _context.Medicos
                .AnyAsync(m => m.MedicoColegiado == medico.MedicoColegiado);

            if (colegiadoExiste)
                ModelState.AddModelError(nameof(medico.MedicoColegiado),
                    "Ya existe un médico registrado con ese número de colegiado.");

            if (!ModelState.IsValid)
            {
                await CargarEspecialidades(medico.EspecialidadId);
                return View(medico);
            }

            try
            {
                _context.Medicos.Add(medico);
                int filas = await _context.SaveChangesAsync();

                TempData["Success"] = $"Médico {medico.MedicoNombres} {medico.MedicoApellidos} registrado exitosamente. Filas afectadas: {filas}";
                return RedirectToAction(nameof(CrearMedico));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException?.Message ?? ex.Message);
                await CargarEspecialidades(medico.EspecialidadId);
                return View(medico);
            }
        }

        // ── Helper ───────────────────────────────────────────────────────────
        private async Task CargarEspecialidades(int? seleccionado = null)
        {
            var especialidades = await _context.Especialidades
                .Where(e => e.EspecialidadEstado)
                .OrderBy(e => e.EspecialidadNombre)
                .ToListAsync();

            ViewBag.Especialidades = new SelectList(
                especialidades,
                nameof(Especialidad.EspecialidadId),
                nameof(Especialidad.EspecialidadNombre),
                seleccionado);
        }
    }
}