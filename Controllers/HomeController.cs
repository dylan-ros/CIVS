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
            var ahora = HoraGuatemala();
            var hoy = ahora.Date;

            ViewBag.AhoraGuatemala = ahora;
            ViewBag.EsMedico = User.IsInRole("Medico");

            // Contadores generales usando fecha de Guatemala
            ViewBag.TotalCitasHoy = await _context.Citas
                .CountAsync(c => c.CitaFechaInicio.Date == hoy);

            ViewBag.TotalProgramadas = await _context.Citas
                .CountAsync(c => c.EstadoCita == EstadoCita.programada
                              && c.CitaFechaInicio.Date == hoy);

            ViewBag.TotalAtendidas = await _context.Citas
                .CountAsync(c => c.EstadoCita == EstadoCita.atendida
                              && c.CitaFechaInicio.Date == hoy);

            ViewBag.TotalPacientes = await _context.Pacientes
                .CountAsync(p => p.PacienteEstado);

            // Si NO es médico, no mostrar citas en inicio
            if (!User.IsInRole("Medico"))
            {
                return View(new List<Cita>());
            }

            // Si SÍ es médico, buscar su usuario
            var username = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(username))
            {
                return View(new List<Cita>());
            }

            var usuario = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UsuarioUsername == username);

            if (usuario == null)
            {
                return View(new List<Cita>());
            }

            // Buscar el médico asociado a ese usuario
            var medico = await _context.Medicos
                .Include(m => m.Especialidad)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.UsuarioId == usuario.UsuarioId);

            if (medico == null)
            {
                return View(new List<Cita>());
            }

            // Traer citas del médico
            // Puedes dejar futuras y actuales, no canceladas.
            var misCitas = await _context.Citas
                .Include(c => c.Paciente)
                .Include(c => c.Medico)
                    .ThenInclude(m => m.Especialidad)
                .Where(c => c.MedicoId == medico.MedicoId
                         && c.EstadoCita != EstadoCita.cancelada)
                .OrderBy(c => c.CitaFechaInicio)
                .ToListAsync();

            return View(misCitas);
        }

        private static DateTime HoraGuatemala()
        {
            try
            {
                var zona = TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zona);
            }
            catch
            {
                try
                {
                    var zona = TimeZoneInfo.FindSystemTimeZoneById("America/Guatemala");
                    return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zona);
                }
                catch
                {
                    return DateTime.UtcNow.AddHours(-6);
                }
            }
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