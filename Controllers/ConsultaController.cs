using CIVS.Data;
using CIVS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CIVS.Controllers
{
    [Authorize]
    public class ConsultaController : Controller
    {
        private readonly AppDbContext _context;

        public ConsultaController(AppDbContext context)
        {
            _context = context;
        }

        // ── Hora local Guatemala ─────────────────────────────────────────────
        // Importante para Azure: evita comparar citas contra UTC.
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

        // ── GET: /Consulta/IniciarConsulta?citaId=5 ──────────────────────────
        [Authorize(Roles = "Administrador,Medico")]
        public async Task<IActionResult> IniciarConsulta(int citaId)
        {
            var cita = await _context.Citas
                .Include(c => c.Paciente)
                .Include(c => c.Medico)
                    .ThenInclude(m => m.Especialidad)
                .FirstOrDefaultAsync(c => c.CitaId == citaId);

            if (cita == null)
                return NotFound();

            if (cita.EstadoCita == EstadoCita.cancelada)
            {
                TempData["Error"] = "No se puede iniciar una consulta de una cita cancelada.";
                return RedirectToAction("Index", "Cita");
            }

            if (cita.EstadoCita == EstadoCita.noAsistio)
            {
                TempData["Error"] = "No se puede iniciar una consulta de una cita marcada como no asistió.";
                return RedirectToAction("Index", "Cita");
            }

            // Si ya tiene consulta → ir directo al detalle.
            var consultaExistente = await _context.Consultas
                .FirstOrDefaultAsync(c => c.CitaId == citaId);

            if (consultaExistente != null)
            {
                return RedirectToAction(nameof(Detalle), new { id = consultaExistente.ConsultaId });
            }

            // Validar horario: solo el médico debe respetar ventana de atención.
            // Administrador puede entrar para soporte/corrección.
            if (User.IsInRole("Medico") && !User.IsInRole("Administrador"))
            {
                var ahora = HoraGuatemala();
                var margenAntes = cita.CitaFechaInicio.AddMinutes(-10);

                // Se deja margen de 30 minutos después de la hora fin para evitar que
                // una cita recién terminada bloquee el acceso por diferencia de segundos/minutos.
                var margenDespues = cita.CitaFechafin.AddMinutes(30);

                if (ahora < margenAntes)
                {
                    TempData["Error"] =
                        $"Esta cita empieza a las {cita.CitaFechaInicio:HH:mm}. " +
                        "Podrás acceder 10 minutos antes.";

                    return RedirectToAction("Index", "Cita");
                }

                if (ahora > margenDespues)
                {
                    TempData["Error"] =
                        $"El horario de esta cita ya finalizó. Hora actual del sistema: {ahora:HH:mm}.";

                    return RedirectToAction("Index", "Cita");
                }
            }

            // Cambiar estado a Atendida.
            cita.EstadoCita = EstadoCita.atendida;
            await _context.SaveChangesAsync();

            // Historial del paciente (últimas 5 consultas).
            var historial = await _context.Consultas
                .Include(c => c.Cita)
                    .ThenInclude(ci => ci.Medico)
                .Where(c => c.Cita.PacienteId == cita.PacienteId)
                .OrderByDescending(c => c.ConsultaFechaRegistro)
                .Take(5)
                .ToListAsync();

            ViewBag.Cita = cita;
            ViewBag.Historial = historial;
            ViewBag.AhoraGuatemala = HoraGuatemala();

            return View();
        }

        // ── POST: /Consulta/IniciarConsulta ───────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Medico")]
        public async Task<IActionResult> IniciarConsulta(
            int CitaId,
            string? ConsultaSignosVitales,
            string? ConsultaNotasClinicas,
            string? ConsultaPlanTratamiento)
        {
            var cita = await _context.Citas
                .FirstOrDefaultAsync(c => c.CitaId == CitaId);

            if (cita == null)
                return NotFound();

            if (cita.EstadoCita == EstadoCita.cancelada || cita.EstadoCita == EstadoCita.noAsistio)
            {
                TempData["Error"] = "No se puede registrar consulta para una cita cancelada o marcada como no asistió.";
                return RedirectToAction("Index", "Cita");
            }

            var consultaExistente = await _context.Consultas
                .FirstOrDefaultAsync(c => c.CitaId == CitaId);

            if (consultaExistente != null)
            {
                return RedirectToAction(nameof(Detalle), new { id = consultaExistente.ConsultaId });
            }

            var ahora = HoraGuatemala();

            var consulta = new Consulta
            {
                CitaId = CitaId,
                ConsultaSignosVitales = ConsultaSignosVitales,
                ConsultaNotasClinicas = ConsultaNotasClinicas,
                ConsultaPlanTratamiento = ConsultaPlanTratamiento,
                ConsultaEstado = true,
                ConsultaFechaRegistro = ahora
            };

            cita.EstadoCita = EstadoCita.atendida;

            _context.Consultas.Add(consulta);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Consulta iniciada.";
            return RedirectToAction(nameof(Detalle), new { id = consulta.ConsultaId });
        }

        // ── GET: /Consulta/Detalle/5 ──────────────────────────────────────────
        [Authorize(Roles = "Administrador,Medico,Laboratorista")]
        public async Task<IActionResult> Detalle(int id)
        {
            var consulta = await _context.Consultas
                .Include(c => c.Cita)
                    .ThenInclude(ci => ci.Paciente)
                .Include(c => c.Cita)
                    .ThenInclude(ci => ci.Medico)
                        .ThenInclude(m => m.Especialidad)
                .FirstOrDefaultAsync(c => c.ConsultaId == id);

            if (consulta == null)
                return NotFound();

            var factura = await _context.Facturas
                .FirstOrDefaultAsync(f => f.CitaId == consulta.CitaId);

            ViewBag.FacturaId = factura?.FacturaId;
            ViewBag.FacturaEstado = factura?.FacturaEstado;
            ViewBag.AhoraGuatemala = HoraGuatemala();

            var diagnosticos = await _context.ConsultaDiagnosticos
                .Include(cd => cd.Diagnostico)
                .Where(cd => cd.ConsultaId == id)
                .ToListAsync();

            var recetas = await _context.Recetas
                .Include(r => r.RecetaDetalles)
                    .ThenInclude(rd => rd.Medicamento)
                .Where(r => r.ConsultaId == id)
                .ToListAsync();

            var ordenes = await _context.OrdenExamenes
                .Include(o => o.Examen)
                .Where(o => o.ConsultaId == id)
                .ToListAsync();

            ViewBag.DiagnosticosJson = System.Text.Json.JsonSerializer.Serialize(
                await _context.Diagnosticos
                    .Where(d => d.DiagnosticoEstado)
                    .Select(d => new
                    {
                        id = d.DiagnosticoId,
                        codigo = d.DiagnosticoCodigo,
                        nombre = d.DiagnosticoNombre
                    })
                    .ToListAsync(),
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                });

            ViewBag.MedicamentosJson = System.Text.Json.JsonSerializer.Serialize(
                await _context.Medicamentos
                    .Where(m => m.MedicamentoEstado)
                    .Select(m => new
                    {
                        id = m.MedicamentoId,
                        nombre = m.MedicamentoNombre,
                        presentacion = m.MedicamentoPresentacion,
                        concentracion = m.MedicamentoConcentracion
                    })
                    .ToListAsync(),
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                });

            ViewBag.Examenes = new SelectList(
                await _context.Examenes
                    .Where(e => e.ExamenEstado)
                    .OrderBy(e => e.ExamenNombre)
                    .ToListAsync(),
                nameof(Examen.ExamenId),
                nameof(Examen.ExamenNombre));

            ViewBag.MedicosSeguimiento = new SelectList(
                await _context.Medicos
                    .Include(m => m.Especialidad)
                    .Where(m => m.MedicoEstado)
                    .OrderBy(m => m.MedicoNombres)
                    .Select(m => new
                    {
                        m.MedicoId,
                        Nombre = $"Dr. {m.MedicoNombres} {m.MedicoApellidos} — {m.Especialidad!.EspecialidadNombre}"
                    })
                    .ToListAsync(),
                "MedicoId",
                "Nombre");

            ViewBag.Diagnosticos = diagnosticos;
            ViewBag.Recetas = recetas;
            ViewBag.Ordenes = ordenes;

            return View(consulta);
        }

        // ── POST: /Consulta/AgregarDiagnostico ───────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Medico")]
        public async Task<IActionResult> AgregarDiagnostico(
            int ConsultaId,
            int DiagnosticoId,
            bool EsPrincipal)
        {
            var consultaExiste = await _context.Consultas.AnyAsync(c => c.ConsultaId == ConsultaId);
            if (!consultaExiste)
                return NotFound();

            bool existe = await _context.ConsultaDiagnosticos
                .AnyAsync(cd => cd.ConsultaId == ConsultaId && cd.DiagnosticoId == DiagnosticoId);

            if (!existe)
            {
                _context.ConsultaDiagnosticos.Add(new ConsultaDiagnostico
                {
                    ConsultaId = ConsultaId,
                    DiagnosticoId = DiagnosticoId,
                    EsPrincipal = EsPrincipal
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Detalle), new { id = ConsultaId });
        }

        // ── POST: /Consulta/EliminarDiagnostico ──────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Medico")]
        public async Task<IActionResult> EliminarDiagnostico(
            int consultaDiagnosticoId,
            int consultaId)
        {
            var cd = await _context.ConsultaDiagnosticos.FindAsync(consultaDiagnosticoId);

            if (cd != null)
            {
                _context.ConsultaDiagnosticos.Remove(cd);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Detalle), new { id = consultaId });
        }

        // ── POST: /Consulta/GenerarReceta ─────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Medico")]
        public async Task<IActionResult> GenerarReceta(
            int ConsultaId,
            string? RecetaObservaciones,
            List<int> MedicamentoId,
            List<string?> Dosis,
            List<string?> Frecuencia,
            List<string?> Duracion,
            List<string?> Indicaciones)
        {
            var consultaExiste = await _context.Consultas.AnyAsync(c => c.ConsultaId == ConsultaId);
            if (!consultaExiste)
                return NotFound();

            var medicamentosValidos = MedicamentoId?
                .Where(id => id > 0)
                .ToList() ?? new List<int>();

            if (!medicamentosValidos.Any())
            {
                TempData["Error"] = "Debe agregar al menos un medicamento.";
                return RedirectToAction(nameof(Detalle), new { id = ConsultaId });
            }

            var receta = new Receta
            {
                ConsultaId = ConsultaId,
                RecetaObservaciones = RecetaObservaciones,
                RecetaFechaEmision = HoraGuatemala(),
                RecetaEstado = true
            };

            _context.Recetas.Add(receta);
            await _context.SaveChangesAsync();

            for (int i = 0; i < medicamentosValidos.Count; i++)
            {
                _context.RecetaDetalles.Add(new RecetaDetalle
                {
                    RecetaId = receta.RecetaId,
                    MedicamentoId = medicamentosValidos[i],
                    Dosis = i < Dosis.Count ? Dosis[i] : null,
                    Frecuencia = i < Frecuencia.Count ? Frecuencia[i] : null,
                    Duracion = i < Duracion.Count ? Duracion[i] : null,
                    Indicaciones = i < Indicaciones.Count ? Indicaciones[i] : null
                });
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Receta generada exitosamente.";
            return RedirectToAction(nameof(Detalle), new { id = ConsultaId });
        }

        // ── POST: /Consulta/GenerarOrden ──────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Medico")]
        public async Task<IActionResult> GenerarOrden(
            int ConsultaId,
            int ExamenId,
            string? Observaciones)
        {
            var consultaExiste = await _context.Consultas.AnyAsync(c => c.ConsultaId == ConsultaId);
            if (!consultaExiste)
                return NotFound();

            var examenExiste = await _context.Examenes.AnyAsync(e => e.ExamenId == ExamenId && e.ExamenEstado);
            if (!examenExiste)
            {
                TempData["Error"] = "Debe seleccionar un examen válido.";
                return RedirectToAction(nameof(Detalle), new { id = ConsultaId });
            }

            _context.OrdenExamenes.Add(new OrdenExamen
            {
                ConsultaId = ConsultaId,
                ExamenId = ExamenId,
                OrdenFecha = HoraGuatemala(),
                OrdenEstado = EstadoOrdenExamen.Solicitado,
                ResultadoTexto = Observaciones
            });

            await _context.SaveChangesAsync();

            TempData["Success"] = "Orden de examen generada.";
            return RedirectToAction(nameof(Detalle), new { id = ConsultaId });
        }

        // ── POST: /Consulta/AgendarSeguimiento ────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Medico")]
        public async Task<IActionResult> AgendarSeguimiento(
            int ConsultaId,
            int PacienteId,
            int MedicoId,
            DateTime FechaInicio,
            DateTime FechaFin,
            string Motivo)
        {
            var consultaExiste = await _context.Consultas.AnyAsync(c => c.ConsultaId == ConsultaId);
            if (!consultaExiste)
                return NotFound();

            var ahora = HoraGuatemala();

            if (FechaInicio <= ahora)
            {
                TempData["Error"] = "La fecha de seguimiento debe ser futura.";
                return RedirectToAction(nameof(Detalle), new { id = ConsultaId });
            }

            if (FechaFin <= FechaInicio)
            {
                TempData["Error"] = "La hora de fin debe ser mayor que la hora de inicio.";
                return RedirectToAction(nameof(Detalle), new { id = ConsultaId });
            }

            bool choque = await _context.Citas.AnyAsync(c =>
                c.MedicoId == MedicoId &&
                c.EstadoCita != EstadoCita.cancelada &&
                c.CitaFechaInicio < FechaFin &&
                c.CitaFechafin > FechaInicio);

            if (choque)
            {
                TempData["Error"] = "El médico ya tiene una cita en ese horario.";
                return RedirectToAction(nameof(Detalle), new { id = ConsultaId });
            }

            _context.Citas.Add(new Cita
            {
                PacienteId = PacienteId,
                MedicoId = MedicoId,
                CitaFechaInicio = FechaInicio,
                CitaFechafin = FechaFin,
                CitaMotivo = Motivo,
                EstadoCita = EstadoCita.programada,
                CitaFechaCreada = ahora
            });

            await _context.SaveChangesAsync();

            TempData["Success"] = "Cita de seguimiento agendada exitosamente.";
            return RedirectToAction(nameof(Detalle), new { id = ConsultaId });
        }

        // ── POST: /Consulta/FinalizarYCobrar ─────────────────────────────────
        // Genera factura pendiente para Corte de Caja.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Medico")]
        public async Task<IActionResult> FinalizarYCobrar(int ConsultaId)
        {
            var consulta = await _context.Consultas
                .Include(c => c.Cita)
                    .ThenInclude(ci => ci.Paciente)
                .FirstOrDefaultAsync(c => c.ConsultaId == ConsultaId);

            if (consulta == null)
                return NotFound();

            var facturaExistente = await _context.Facturas
                .FirstOrDefaultAsync(f => f.CitaId == consulta.CitaId);

            if (facturaExistente != null)
            {
                TempData["Success"] = facturaExistente.FacturaEstado == EstadoFactura.Pagada
                    ? "La consulta ya tiene una factura pagada."
                    : "La factura ya fue enviada a Corte de Caja y está pendiente de cobro.";

                return RedirectToAction(nameof(Detalle), new { id = ConsultaId });
            }

            var ordenes = await _context.OrdenExamenes
                .Include(o => o.Examen)
                .Where(o => o.ConsultaId == ConsultaId)
                .ToListAsync();

            var recetaDetalles = await _context.RecetaDetalles
                .Include(rd => rd.Medicamento)
                .Include(rd => rd.Receta)
                .Where(rd => rd.Receta.ConsultaId == ConsultaId)
                .ToListAsync();

            var detalles = new List<FacturaDetalle>();
            decimal subtotal = 0;

            // Consulta médica.
            detalles.Add(new FacturaDetalle
            {
                DetalleDescripcion = "Consulta médica",
                DetalleCantidad = 1,
                DetallePrecioUnitario = 150,
                DetalleDescuento = 0,
                DetalleTotalLinea = 150
            });
            subtotal += 150;

            // Exámenes.
            foreach (var o in ordenes)
            {
                var precio = o.Examen.ExamenPrecio ?? 0;

                detalles.Add(new FacturaDetalle
                {
                    ExamenId = o.ExamenId,
                    DetalleDescripcion = o.Examen.ExamenNombre,
                    DetalleCantidad = 1,
                    DetallePrecioUnitario = precio,
                    DetalleDescuento = 0,
                    DetalleTotalLinea = precio
                });

                subtotal += precio;
            }

            // Medicamentos.
            foreach (var rd in recetaDetalles)
            {
                var precio = rd.Medicamento.MedicamentoPrecio ?? 0;
                var dosis = string.IsNullOrWhiteSpace(rd.Dosis) ? "" : $" — {rd.Dosis}";

                detalles.Add(new FacturaDetalle
                {
                    MedicamentoId = rd.MedicamentoId,
                    DetalleDescripcion = $"{rd.Medicamento.MedicamentoNombre}{dosis}",
                    DetalleCantidad = 1,
                    DetallePrecioUnitario = precio,
                    DetalleDescuento = 0,
                    DetalleTotalLinea = precio
                });

                subtotal += precio;
            }

            int correlativo = await _context.Facturas.CountAsync() + 1;
            var ahora = HoraGuatemala();

            var factura = new Factura
            {
                FacturaNumero = $"FAC-{correlativo:D6}",
                PacienteId = consulta.Cita.PacienteId,
                CitaId = consulta.CitaId,
                FacturaFecha = ahora,
                FacturaSubtotal = subtotal,
                FacturaDescuento = 0,
                FacturaImpuesto = 0,
                FacturaTotal = subtotal,
                FacturaEstado = EstadoFactura.Emitida,
                FacturaFechaRegistro = ahora
            };

            _context.Facturas.Add(factura);
            await _context.SaveChangesAsync();

            foreach (var d in detalles)
            {
                d.FacturaId = factura.FacturaId;
                _context.FacturaDetalle.Add(d);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"Consulta finalizada. La factura {factura.FacturaNumero} fue enviada a Corte de Caja como pendiente de cobro.";

            return RedirectToAction(nameof(Detalle), new { id = ConsultaId });
        }

        // ── GET: /Consulta/Pago/5 ─────────────────────────────────────────────
        [Authorize(Roles = "Administrador,Contabilidad")]
        public async Task<IActionResult> Pago(int id)
        {
            var factura = await _context.Facturas
                .Include(f => f.Paciente)
                .Include(f => f.Detalles)
                .Include(f => f.Cita)
                .FirstOrDefaultAsync(f => f.FacturaId == id);

            if (factura == null)
                return NotFound();

            return View(factura);
        }

        // ── POST: /Consulta/RegistrarPago ─────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Contabilidad")]
        public async Task<IActionResult> RegistrarPago(int FacturaId)
        {
            var factura = await _context.Facturas
                .Include(f => f.Cita)
                .FirstOrDefaultAsync(f => f.FacturaId == FacturaId);

            if (factura == null)
                return NotFound();

            if (factura.FacturaEstado == EstadoFactura.Pagada)
            {
                TempData["Success"] = $"La factura {factura.FacturaNumero} ya estaba pagada.";
                return RedirectToAction(nameof(PagoCompletado), new { id = FacturaId });
            }

            factura.FacturaEstado = EstadoFactura.Pagada;

            if (factura.Cita != null)
                factura.Cita.EstadoCita = EstadoCita.atendida;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Pago registrado. Factura {factura.FacturaNumero} completada.";

            return RedirectToAction(nameof(PagoCompletado), new { id = FacturaId });
        }

        // ── GET: /Consulta/PagoCompletado/5 ──────────────────────────────────
        [Authorize(Roles = "Administrador,Contabilidad")]
        public async Task<IActionResult> PagoCompletado(int id)
        {
            var factura = await _context.Facturas
                .Include(f => f.Paciente)
                .Include(f => f.Detalles)
                .Include(f => f.Cita)
                    .ThenInclude(c => c!.Medico)
                .FirstOrDefaultAsync(f => f.FacturaId == id);

            if (factura == null)
                return NotFound();

            return View(factura);
        }
    }
}