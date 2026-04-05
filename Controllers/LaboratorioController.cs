using CIVS.Data;
using CIVS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CIVS.Controllers
{
    [Authorize(Roles = "Administrador,Recepcionista,Medico,Laboratorista")]
    public class LaboratorioController : Controller
    {
        private readonly AppDbContext _context;

        public LaboratorioController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? q)
        {
            var query = _context.OrdenExamenes
                .Include(o => o.Examen)
                .Include(o => o.Consulta)
                    .ThenInclude(c => c.Cita)
                        .ThenInclude(ci => ci.Paciente)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(o =>
                    o.Examen.ExamenNombre.Contains(q) ||
                    o.Consulta.Cita.Paciente.PacienteNombres.Contains(q) ||
                    o.Consulta.Cita.Paciente.PacienteApellido.Contains(q) ||
                    (o.Consulta.Cita.Paciente.PacienteNombres + " " +
                     o.Consulta.Cita.Paciente.PacienteApellido).Contains(q) ||
                    o.Consulta.Cita.Paciente.PacienteDPI.Contains(q));
            }

            var ordenes = await query
               .OrderByDescending(o => o.OrdenFecha)
               .ToListAsync();

            ViewBag.Q = q;
            return View(ordenes);
        }

        // ── GET: /Laboratorio/CrearOrden ─────────────────────────────────────
        public async Task<IActionResult> CrearOrden()
        {
            await CargarCombos();
            return View();
        }

        // ── POST: /Laboratorio/CrearOrden ────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearOrden(OrdenExamen orden)
        {
            if (!ModelState.IsValid)
            {
                await CargarCombos(orden.ConsultaId, orden.ExamenId);
                return View(orden);
            }

            orden.OrdenEstado = EstadoOrdenExamen.Solicitado;
            orden.OrdenFecha = DateTime.UtcNow;

            _context.OrdenExamenes.Add(orden);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Orden de examen creada exitosamente.";
            return RedirectToAction(nameof(CrearOrden));
        }

        // ── GET: /Laboratorio/Detalle/5 ──────────────────────────────────────
        public async Task<IActionResult> Detalle(int id)
        {
            var orden = await _context.OrdenExamenes
                .Include(o => o.Examen)
                .Include(o => o.OrdenDetalles)
                .Include(o => o.Consulta)
                    .ThenInclude(c => c.Cita)
                        .ThenInclude(ci => ci.Paciente)
                .Include(o => o.Consulta)
                    .ThenInclude(c => c.Cita)
                        .ThenInclude(ci => ci.Medico)
                .FirstOrDefaultAsync(o => o.OrdenExamenId == id);

            if (orden == null)
                return NotFound();

            return View(orden);
        }

        // ── POST: /Laboratorio/CambiarEstado ────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id, EstadoOrdenExamen estado)
        {
            var orden = await _context.OrdenExamenes.FindAsync(id);

            if (orden == null)
                return NotFound();

            orden.OrdenEstado = estado;

            if (estado == EstadoOrdenExamen.Entregado)
                orden.ResultadoFecha = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Estado actualizado a: {estado}.";
            return RedirectToAction(nameof(Detalle), new { id });
        }

        // ── GET: /Laboratorio/CrearExamen ─────────────────────────────────────
        public IActionResult CrearExamen()
        {
            return View();
        }

        // ── POST: /Laboratorio/CrearExamen ────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearExamen(Examen examen)
        {
            bool nombreExiste = await _context.Examenes
                .AnyAsync(e => e.ExamenNombre == examen.ExamenNombre);

            if (nombreExiste)
                ModelState.AddModelError(nameof(examen.ExamenNombre),
                    "Ya existe un examen registrado con ese nombre.");

            if (!ModelState.IsValid)
                return View(examen);

            examen.ExamenEstado = true;
            examen.ExamenFechaRegistro = DateTime.UtcNow;

            _context.Examenes.Add(examen);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Examen '{examen.ExamenNombre}' registrado exitosamente.";
            return RedirectToAction(nameof(CrearExamen));
        }

        // ── Helper ────────────────────────────────────────────────────────────
        private async Task CargarCombos(int? consultaId = null, int? examenId = null)
        {
            var consultas = await _context.Consultas
                .Include(c => c.Cita)
                    .ThenInclude(ci => ci.Paciente)
                .Where(c => c.ConsultaEstado)
                .OrderByDescending(c => c.ConsultaFechaRegistro)
                .ToListAsync();

            var examenes = await _context.Examenes
                .Where(e => e.ExamenEstado)
                .OrderBy(e => e.ExamenNombre)
                .ToListAsync();

            ViewBag.Consultas = new SelectList(
                consultas.Select(c => new {
                    c.ConsultaId,
                    Descripcion = $"{c.Cita.Paciente.PacienteNombres} {c.Cita.Paciente.PacienteApellido} — {c.ConsultaFechaRegistro:dd/MM/yyyy}"
                }),
                "ConsultaId", "Descripcion", consultaId);

            ViewBag.Examenes = new SelectList(
                examenes, nameof(Examen.ExamenId),
                nameof(Examen.ExamenNombre), examenId);
        }

        // ── GET: /Laboratorio/CrearReceta ─────────────────────────────────────────
        public async Task<IActionResult> CrearReceta()
        {
            await CargarConsultas();
            await CargarMedicamentosJson();
            return View();
        }


        // ── POST: /Laboratorio/CrearReceta ────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearReceta(
            int ConsultaId,
            string? RecetaObservaciones,
            List<int> MedicamentoId,
            List<string?> Dosis,
            List<string?> Frecuencia,
            List<string?> Duracion,
            List<string?> Indicaciones)
        {
            if (ConsultaId == 0)
            {
                ModelState.AddModelError("", "Debe seleccionar una consulta.");
                await CargarConsultas();
                await CargarMedicamentosJson();
                return View();
            }

            if (MedicamentoId == null || !MedicamentoId.Any())
            {
                ModelState.AddModelError("", "Debe agregar al menos un medicamento a la receta.");
                await CargarConsultas(ConsultaId);
                await CargarMedicamentosJson();
                return View();
            }

            // Crear encabezado
            var receta = new Receta
            {
                ConsultaId = ConsultaId,
                RecetaObservaciones = RecetaObservaciones,
                RecetaFechaEmision = DateTime.UtcNow,
                RecetaEstado = true
            };

            _context.Recetas.Add(receta);
            await _context.SaveChangesAsync();

            // Crear líneas de detalle
            for (int i = 0; i < MedicamentoId.Count; i++)
            {
                _context.RecetaDetalles.Add(new RecetaDetalle
                {
                    RecetaId = receta.RecetaId,
                    MedicamentoId = MedicamentoId[i],
                    Dosis = i < Dosis.Count ? Dosis[i] : null,
                    Frecuencia = i < Frecuencia.Count ? Frecuencia[i] : null,
                    Duracion = i < Duracion.Count ? Duracion[i] : null,
                    Indicaciones = i < Indicaciones.Count ? Indicaciones[i] : null
                });
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Receta médica creada exitosamente.";
            return RedirectToAction(nameof(CrearReceta));
        }

        private async Task CargarConsultas(int? seleccionado = null)
        {
            var consultas = await _context.Consultas
                .Include(c => c.Cita)
                    .ThenInclude(ci => ci.Paciente)
                .Where(c => c.ConsultaEstado)
                .OrderByDescending(c => c.ConsultaFechaRegistro)
                .ToListAsync();

            ViewBag.Consultas = new SelectList(
                consultas.Select(c => new {
                    c.ConsultaId,
                    Descripcion = $"{c.Cita.Paciente.PacienteNombres} {c.Cita.Paciente.PacienteApellido} — {c.ConsultaFechaRegistro:dd/MM/yyyy}"
                }),
                "ConsultaId", "Descripcion", seleccionado);
        }

        // ── Helper: serializar medicamentos activos a JSON para el script ─────────
        private async Task CargarMedicamentosJson()
        {
            var medicamentos = await _context.Medicamentos
                .Where(m => m.MedicamentoEstado)
                .OrderBy(m => m.MedicamentoNombre)
                .Select(m => new {
                    m.MedicamentoId,
                    m.MedicamentoNombre,
                    m.MedicamentoPresentacion,
                    m.MedicamentoConcentracion
                })
                .ToListAsync();

            ViewBag.MedicamentosJson = System.Text.Json.JsonSerializer.Serialize(
                medicamentos,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                });
        }





    }
}