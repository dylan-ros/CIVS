using CIVS.Data;
using CIVS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace CIVS.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var ahora = DateTime.UtcNow;

            // Contadores generales
            ViewBag.TotalCitasHoy = await _context.Citas
                .CountAsync(c => c.CitaFechaInicio.Date == ahora.Date);

            ViewBag.TotalProgramadas = await _context.Citas
                .CountAsync(c => c.EstadoCita == EstadoCita.programada
                              && c.CitaFechaInicio.Date == ahora.Date);

            ViewBag.TotalAtendidas = await _context.Citas
                .CountAsync(c => c.EstadoCita == EstadoCita.atendida);

            ViewBag.TotalPacientes = await _context.Pacientes
                .CountAsync(p => p.PacienteEstado);

            // Si NO es médico, no mostrar citas en inicio
            if (!User.IsInRole("Medico"))
            {
                return View(new List<Cita>());
            }

            // Si SÍ es médico, buscar su usuario
            var username = User.Identity!.Name;

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.UsuarioUsername == username);

            if (usuario == null)
            {
                return View(new List<Cita>());
            }

            // Buscar el médico asociado a ese usuario
            var medico = await _context.Medicos
                .Include(m => m.Especialidad)
                .FirstOrDefaultAsync(m => m.UsuarioId == usuario.UsuarioId);

            if (medico == null)
            {
                return View(new List<Cita>());
            }

            // Traer SOLO las citas de ese médico
            var misCitas = await _context.Citas
                .Include(c => c.Paciente)
                .Include(c => c.Medico)
                    .ThenInclude(m => m.Especialidad)
                .Where(c => c.MedicoId == medico.MedicoId
                         && c.EstadoCita != EstadoCita.cancelada)
                .OrderBy(c => c.CitaFechaInicio)
                .ToListAsync();

            ViewBag.EsMedico = true;

            return View(misCitas);
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}