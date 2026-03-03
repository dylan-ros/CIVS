using CIVS.Data;
using CIVS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CIVS.Controllers
{
    public class CitaController : Controller
    {
        private readonly AppDbContext _context;

        public CitaController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Cita
        public async Task<IActionResult> Index()
        {
            var citas = await _context.Citas
                .Include(c => c.Paciente)
                .Include(c => c.Medico)
                .OrderByDescending(c => c.CitaFechaInicio)
                .ToListAsync();

            return View(citas); // Busca: Views/Cita/Index.cshtml
        }

        // GET: Cita/CrearCitas
        public async Task<IActionResult> CrearCitas()
        {
            await CargarCombos();
            return View(); // Busca: Views/Cita/CrearCitas.cshtml
        }

        // POST: Cita/CrearCitas
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearCitas(Cita cita)
        {
            // ⚠️ Corrige el nombre si tu propiedad se llama distinto (ideal: CitaFechaFin)
            if (cita.CitaFechafin <= cita.CitaFechaInicio)
                ModelState.AddModelError(nameof(Cita.CitaFechafin), "La hora final debe ser mayor que la hora inicial.");

            if (!ModelState.IsValid)
            {
                await CargarCombos(cita.PacienteId, cita.MedicoId);
                return View(cita); // Vuelve a Views/Cita/CrearCitas.cshtml
            }

            _context.Citas.Add(cita);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task CargarCombos(int? pacienteId = null, int? medicoId = null)
        {
            var pacientes = await _context.Pacientes
                .Where(p => p.PacienteEstado == true)
                .OrderBy(p => p.PacienteNombres)
                .ToListAsync();

            var medicos = await _context.Medicos
                .Where(m => m.MedicoEstado == true)
                .OrderBy(m => m.MedicoNombres)
                .ToListAsync();

            ViewBag.Pacientes = new SelectList(pacientes, "PacienteId", "PacienteNombres", pacienteId);
            ViewBag.Medicos = new SelectList(medicos, "MedicoId", "MedicoNombres", medicoId);
        }
    }
}