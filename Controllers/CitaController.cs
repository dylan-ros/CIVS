using CIVS.Data;
using CIVS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CIVS.Controllers
{
    [Authorize]
    public class CitaController : Controller
    {
        private readonly AppDbContext _context;

        public CitaController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Cita
        public async Task<IActionResult> Index(string? q, string? estado)
        {
            var query = _context.Citas
                .Include(c => c.Paciente)
                .Include(c => c.Medico)
                .AsQueryable();

            // Si el usuario es Médico, mostrar solo sus citas
            if (User.IsInRole("Medico"))
            {
                var username = User.Identity!.Name;
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.UsuarioUsername == username);

                if (usuario != null)
                {
                    var medico = await _context.Medicos
                        .FirstOrDefaultAsync(m => m.UsuarioId == usuario.UsuarioId);

                    if (medico != null)
                        query = query.Where(c => c.MedicoId == medico.MedicoId);
                    else
                        query = query.Where(c => false);
                }
            }

            // Filtros de búsqueda
            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(c =>
                    c.Paciente.PacienteNombres.Contains(q) ||
                    c.Paciente.PacienteApellido.Contains(q) ||
                    (c.Paciente.PacienteNombres + " " + c.Paciente.PacienteApellido).Contains(q) ||
                    c.Paciente.PacienteDPI.Contains(q) ||
                    c.Medico.MedicoNombres.Contains(q) ||
                    c.Medico.MedicoApellidos.Contains(q) ||
                    c.CitaMotivo.Contains(q));
            }

            if (!string.IsNullOrWhiteSpace(estado) &&
                Enum.TryParse<EstadoCita>(estado, out var estadoEnum))
                query = query.Where(c => c.EstadoCita == estadoEnum);

            // Ejecutar query — para médicos siempre mostrar, para otros solo si hay filtros
            List<Cita> citas;

            if (User.IsInRole("Medico"))
            {
                // Médico siempre ve sus citas sin necesidad de buscar
                citas = await query
                    .OrderBy(c => c.CitaFechaInicio)
                    .ToListAsync();
            }
            else if (!string.IsNullOrWhiteSpace(q) || !string.IsNullOrWhiteSpace(estado))
            {
                // Otros roles solo ven resultados si buscaron
                citas = await query
                    .OrderByDescending(c => c.CitaFechaInicio)
                    .ToListAsync();
            }
            else
            {
                citas = new List<Cita>();
            }

            ViewBag.Q = q;
            ViewBag.Estado = estado;
            ViewBag.EsMedico = User.IsInRole("Medico");
            ViewBag.AhoraGuatemala = HoraGuatemala();

            return View(citas);
        }

        // GET: /Cita/CrearCitas
        public async Task<IActionResult> CrearCitas()
        {
            await CargarCombos();
            await CargarEspecialidades();
            return View();
        }

        // GET: /Cita/EventosCalendario
        [HttpGet]
        public async Task<IActionResult> EventosCalendario(int? medicoId, int? especialidadId)
        {
            var citasQuery = _context.Citas
                .Include(c => c.Paciente)
                .Include(c => c.Medico)
                .Where(c => c.EstadoCita != EstadoCita.cancelada);

            if (medicoId.HasValue)
                citasQuery = citasQuery.Where(c => c.MedicoId == medicoId.Value);

            if (especialidadId.HasValue)
                citasQuery = citasQuery.Where(c => c.Medico.EspecialidadId == especialidadId.Value);

            var citas = await citasQuery.ToListAsync();

            var horariosQuery = _context.HorarioMedicos
                .Include(h => h.Medico)
                .Where(h => h.MedicoHorarioDisponible);

            if (medicoId.HasValue)
                horariosQuery = horariosQuery.Where(h => h.MedicoId == medicoId.Value);

            if (especialidadId.HasValue)
                horariosQuery = horariosQuery.Where(h => h.Medico.EspecialidadId == especialidadId.Value);

            var horarios = await horariosQuery.ToListAsync();

            var eventos = new List<object>();

            foreach (var c in citas)
            {
                string color = c.EstadoCita switch
                {
                    EstadoCita.programada => "#f59e0b",
                    EstadoCita.confirmada => "#3b82f6",
                    EstadoCita.atendida => "#10b981",
                    EstadoCita.noAsistio => "#6b7280",
                    _ => "#6b7280"
                };

                eventos.Add(new
                {
                    id = c.CitaId,
                    title = $"{c.Paciente.PacienteNombres} {c.Paciente.PacienteApellido}",
                    start = c.CitaFechaInicio.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = c.CitaFechafin.ToString("yyyy-MM-ddTHH:mm:ss"),
                    color,
                    extendedProps = new
                    {
                        tipo = "cita",
                        medico = $"Dr. {c.Medico.MedicoNombres} {c.Medico.MedicoApellidos}",
                        paciente = $"{c.Paciente.PacienteNombres} {c.Paciente.PacienteApellido}",
                        motivo = c.CitaMotivo,
                        estado = c.EstadoCita.ToString()
                    }
                });
            }

            foreach (var h in horarios)
            {
                eventos.Add(new
                {
                    id = $"h-{h.HorarioMedicoId}",
                    title = $"Disponible — Dr. {h.Medico.MedicoNombres} {h.Medico.MedicoApellidos}",
                    start = h.MedicoHorarioInicio.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = h.MedicoHorarioFin.ToString("yyyy-MM-ddTHH:mm:ss"),
                    color = "#d1fae5",
                    textColor = "#065f46",
                    display = "background",
                    extendedProps = new { tipo = "horario" }
                });
            }

            return Json(eventos);
        }

        // POST: /Cita/CrearCitas
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearCitas(Cita cita)
        {
            ModelState.Remove(nameof(Cita.PacienteId));
            ModelState.Remove(nameof(Cita.MedicoId));
            ModelState.Remove(nameof(Cita.CitaFechaInicio));
            ModelState.Remove(nameof(Cita.CitaFechafin));
            ModelState.Remove(nameof(Cita.CitaMotivo));

            if (cita.PacienteId == 0)
                ModelState.AddModelError("", "Debe seleccionar un paciente.");

            if (cita.MedicoId == 0)
                ModelState.AddModelError("", "Debe seleccionar un médico.");

            var ahora = HoraGuatemala();

            if (cita.CitaFechaInicio <= ahora)
                ModelState.AddModelError(nameof(Cita.CitaFechaInicio),
                    "No se puede agendar una cita en una fecha y hora pasada.");

            if (cita.CitaFechafin <= cita.CitaFechaInicio)
                ModelState.AddModelError(nameof(Cita.CitaFechafin),
                    "La hora de fin debe ser mayor que la hora de inicio.");

            if (cita.CitaFechafin > cita.CitaFechaInicio &&
                (cita.CitaFechafin - cita.CitaFechaInicio).TotalMinutes < 15)
                ModelState.AddModelError(nameof(Cita.CitaFechafin),
                    "La cita debe durar al menos 15 minutos.");

            var diaSemana = cita.CitaFechaInicio.DayOfWeek;
            if (diaSemana == DayOfWeek.Sunday)
                ModelState.AddModelError(nameof(Cita.CitaFechaInicio),
                    "Solo se pueden agendar citas de lunes a sábado.");

            if (cita.MedicoId > 0)
            {
                bool choque = await _context.Citas.AnyAsync(c =>
                    c.MedicoId == cita.MedicoId &&
                    c.EstadoCita != EstadoCita.cancelada &&
                    c.CitaFechaInicio < cita.CitaFechafin &&
                    c.CitaFechafin > cita.CitaFechaInicio);

                if (choque)
                    ModelState.AddModelError("",
                        "El médico ya tiene una cita en ese horario.");
            }

            if (!ModelState.IsValid)
            {
                await CargarCombos(cita.PacienteId, cita.MedicoId);
                await CargarEspecialidades();
                return View(cita);
            }

            cita.EstadoCita = EstadoCita.programada;
            cita.CitaFechaCreada = HoraGuatemala();

            _context.Citas.Add(cita);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cita programada exitosamente.";
            return RedirectToAction(nameof(CrearCitas));
        }

        // POST: /Cita/CambiarEstado
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id, EstadoCita estado)
        {
            var cita = await _context.Citas.FindAsync(id);
            if (cita == null) return NotFound();

            cita.EstadoCita = estado;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Estado de cita actualizado a: {estado}.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Cita/CancelarCitaAjax
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarCitaAjax(int id, string motivoCancelacion)
        {
            if (string.IsNullOrWhiteSpace(motivoCancelacion))
            {
                return Json(new
                {
                    ok = false,
                    mensaje = "Debe ingresar el motivo de cancelación."
                });
            }

            var cita = await _context.Citas.FindAsync(id);

            if (cita == null)
            {
                return Json(new
                {
                    ok = false,
                    mensaje = "No se encontró la cita seleccionada."
                });
            }

            if (cita.EstadoCita == EstadoCita.atendida)
            {
                return Json(new
                {
                    ok = false,
                    mensaje = "No se puede cancelar una cita que ya fue atendida."
                });
            }

            cita.EstadoCita = EstadoCita.cancelada;
            cita.CitaMotivoCancelacion = motivoCancelacion.Trim();
            cita.CitaFechaCancelacion = HoraGuatemala();

            await _context.SaveChangesAsync();

            return Json(new
            {
                ok = true,
                mensaje = "Cita cancelada exitosamente."
            });
        }

        // ── Helpers ───────────────────────────────────────────────────────────

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

        private async Task CargarCombos(int? pacienteId = null, int? medicoId = null)
        {
            var pacientes = await _context.Pacientes
                .Where(p => p.PacienteEstado)
                .OrderBy(p => p.PacienteNombres)
                .ToListAsync();

            ViewBag.PacientesJson = System.Text.Json.JsonSerializer.Serialize(
                pacientes.Select(p => new {
                    id = p.PacienteId,
                    nombre = $"{p.PacienteNombres} {p.PacienteApellido}",
                    dpi = p.PacienteDPI
                }),
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                });

            var medicos = await _context.Medicos
                .Include(m => m.Especialidad)
                .Where(m => m.MedicoEstado)
                .OrderBy(m => m.MedicoNombres)
                .ToListAsync();

            ViewBag.MedicosJson = System.Text.Json.JsonSerializer.Serialize(
                medicos.Select(m => new {
                    id = m.MedicoId,
                    nombre = $"Dr. {m.MedicoNombres} {m.MedicoApellidos}",
                    colegiado = m.MedicoColegiado,
                    especialidad = m.Especialidad?.EspecialidadNombre ?? "",
                    especialidadId = m.EspecialidadId
                }),
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                });

            ViewBag.Pacientes = new SelectList(Enumerable.Empty<object>());
            ViewBag.MedicosLista = medicos;

            ViewBag.MedicoIdSeleccionado = medicoId;
            ViewBag.PacienteIdSeleccionado = pacienteId;
        }

        private async Task CargarEspecialidades()
        {
            ViewBag.Especialidades = await _context.Especialidades
                .Where(e => e.EspecialidadEstado)
                .OrderBy(e => e.EspecialidadNombre)
                .ToListAsync();
        }
    }
}