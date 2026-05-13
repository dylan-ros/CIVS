using CIVS.Data;
using CIVS.Models;
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
    }
}