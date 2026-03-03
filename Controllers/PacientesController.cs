using CIVS.Data;
using CIVS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CIVS.Controllers
{
    public class PacienteController : Controller
    {
        private readonly AppDbContext _context;

        public PacienteController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Paciente
        public async Task<IActionResult> Index()
        {
            var pacientes = await _context.Pacientes
                .OrderByDescending(p => p.PacienteFechaRegistro)
                .ToListAsync();

            return View(pacientes); // Views/Paciente/Index.cshtml RUTA
        }

        // GET: Paciente/CrearPaciente
        public IActionResult CrearPaciente()
        {
            return View(); // Views/Paciente/CrearPaciente.cshtml RUTA
        }

        // POST: Paciente/CrearPaciente PARA CREAR PACIENTE 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearPaciente(Paciente paciente)
        {
            if (!ModelState.IsValid)
                return View(paciente);

            _context.Pacientes.Add(paciente);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Paciente creado exitosamente.";

            return RedirectToAction(nameof(CrearPaciente));
        }
    }
}