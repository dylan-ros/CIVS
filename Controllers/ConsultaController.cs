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

        // ── GET: /Consulta/IniciarConsulta?citaId=5 ──────────────────────────
        [Authorize(Roles = "Administrador,Medico")]
        public async Task<IActionResult> IniciarConsulta(int citaId)
        {
            var cita = await _context.Citas
                .Include(c => c.Paciente)
                .Include(c => c.Medico)
                    .ThenInclude(m => m.Especialidad)
                .FirstOrDefaultAsync(c => c.CitaId == citaId);

            if (cita == null) return NotFound();

            // Si ya tiene consulta → ir directo al detalle
            var consultaExistente = await _context.Consultas
                .FirstOrDefaultAsync(c => c.CitaId == citaId);

            if (consultaExistente != null)
                return RedirectToAction(nameof(Detalle),
                    new { id = consultaExistente.ConsultaId });

            // ── Validar horario: solo puede entrar en el horario de la cita ──
            if (User.IsInRole("Medico"))
            {
                var ahora = DateTime.Now;
                var margenAntes = cita.CitaFechaInicio.AddMinutes(-10);
                var margenDespues = cita.CitaFechafin;

                if (ahora < margenAntes)
                {
                    TempData["Error"] =
                        $"Esta cita empieza a las {cita.CitaFechaInicio:HH:mm}. " +
                        $"Podrás acceder 10 minutos antes.";
                    return RedirectToAction("Index", "Cita");
                }

                if (ahora > margenDespues)
                {
                    TempData["Error"] =
                        "El horario de esta cita ya finalizó.";
                    return RedirectToAction("Index", "Cita");
                }
            }

            // Cambiar estado a Atendida
            cita.EstadoCita = EstadoCita.atendida;
            await _context.SaveChangesAsync();

            // Historial del paciente (últimas 5 consultas)
            var historial = await _context.Consultas
                .Include(c => c.Cita)
                    .ThenInclude(ci => ci.Medico)
                .Where(c => c.Cita.PacienteId == cita.PacienteId)
                .OrderByDescending(c => c.ConsultaFechaRegistro)
                .Take(5)
                .ToListAsync();

            ViewBag.Cita = cita;
            ViewBag.Historial = historial;
            return View();
        }

        // ── POST: /Consulta/IniciarConsulta ───────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Medico")]
        public async Task<IActionResult> IniciarConsulta(
            int CitaId,
            string ConsultaSignosVitales,
            string ConsultaNotasClinicas,
            string ConsultaPlanTratamiento)
        {
            var consulta = new Consulta
            {
                CitaId = CitaId,
                ConsultaSignosVitales = ConsultaSignosVitales,
                ConsultaNotasClinicas = ConsultaNotasClinicas,
                ConsultaPlanTratamiento = ConsultaPlanTratamiento,
                ConsultaEstado = true,
                ConsultaFechaRegistro = DateTime.UtcNow
            };

            _context.Consultas.Add(consulta);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Consulta iniciada.";
            return RedirectToAction(nameof(Detalle),
                new { id = consulta.ConsultaId });
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

            if (consulta == null) return NotFound();

            // ¿Ya tiene factura?
            var factura = await _context.Facturas
                .FirstOrDefaultAsync(f => f.CitaId == consulta.CitaId);
            ViewBag.FacturaId = factura?.FacturaId;

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
                    .Select(d => new {
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
                    .Select(m => new {
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
                    .Select(m => new {
                        m.MedicoId,
                        Nombre = $"Dr. {m.MedicoNombres} {m.MedicoApellidos} — {m.Especialidad!.EspecialidadNombre}"
                    })
                    .ToListAsync(),
                "MedicoId", "Nombre");

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
            int ConsultaId, int DiagnosticoId, bool EsPrincipal)
        {
            bool existe = await _context.ConsultaDiagnosticos
                .AnyAsync(cd => cd.ConsultaId == ConsultaId &&
                                cd.DiagnosticoId == DiagnosticoId);

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
            int consultaDiagnosticoId, int consultaId)
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
            if (MedicamentoId == null || !MedicamentoId.Any())
            {
                TempData["Error"] = "Debe agregar al menos un medicamento.";
                return RedirectToAction(nameof(Detalle), new { id = ConsultaId });
            }

            var receta = new Receta
            {
                ConsultaId = ConsultaId,
                RecetaObservaciones = RecetaObservaciones,
                RecetaFechaEmision = DateTime.UtcNow,
                RecetaEstado = true
            };

            _context.Recetas.Add(receta);
            await _context.SaveChangesAsync();

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
            TempData["Success"] = "Receta generada exitosamente.";
            return RedirectToAction(nameof(Detalle), new { id = ConsultaId });
        }

        // ── POST: /Consulta/GenerarOrden ──────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Medico")]
        public async Task<IActionResult> GenerarOrden(
            int ConsultaId, int ExamenId, string? Observaciones)
        {
            _context.OrdenExamenes.Add(new OrdenExamen
            {
                ConsultaId = ConsultaId,
                ExamenId = ExamenId,
                OrdenFecha = DateTime.UtcNow,
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
            int ConsultaId, int PacienteId, int MedicoId,
            DateTime FechaInicio, DateTime FechaFin, string Motivo)
        {
            if (FechaInicio <= DateTime.UtcNow)
            {
                TempData["Error"] = "La fecha de seguimiento debe ser futura.";
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
                CitaFechaCreada = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "Cita de seguimiento agendada exitosamente.";
            return RedirectToAction(nameof(Detalle), new { id = ConsultaId });
        }

        // ── POST: /Consulta/FinalizarYCobrar ─────────────────────────────────
        // Genera la factura y redirige a la pantalla de pago
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Medico,Cajero")]
        public async Task<IActionResult> FinalizarYCobrar(int ConsultaId)
        {
            var consulta = await _context.Consultas
                .Include(c => c.Cita)
                .FirstOrDefaultAsync(c => c.ConsultaId == ConsultaId);

            if (consulta == null) return NotFound();

            // Si ya tiene factura → ir directo al pago
            var facturaExistente = await _context.Facturas
                .FirstOrDefaultAsync(f => f.CitaId == consulta.CitaId);

            if (facturaExistente != null)
                return RedirectToAction(nameof(Pago),
                    new { id = facturaExistente.FacturaId });

            // Calcular ítems
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

            // Consulta médica
            detalles.Add(new FacturaDetalle
            {
                DetalleDescripcion = "Consulta médica",
                DetalleCantidad = 1,
                DetallePrecioUnitario = 150,
                DetalleDescuento = 0,
                DetalleTotalLinea = 150
            });
            subtotal += 150;

            // Exámenes
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

            // Medicamentos
            foreach (var rd in recetaDetalles)
            {
                var precio = rd.Medicamento.MedicamentoPrecio ?? 0;
                detalles.Add(new FacturaDetalle
                {
                    MedicamentoId = rd.MedicamentoId,
                    DetalleDescripcion = $"{rd.Medicamento.MedicamentoNombre} — {rd.Dosis}",
                    DetalleCantidad = 1,
                    DetallePrecioUnitario = precio,
                    DetalleDescuento = 0,
                    DetalleTotalLinea = precio
                });
                subtotal += precio;
            }

            int correlativo = await _context.Facturas.CountAsync() + 1;

            var factura = new Factura
            {
                FacturaNumero = $"FAC-{correlativo:D6}",
                PacienteId = consulta.Cita.PacienteId,
                CitaId = consulta.CitaId,
                FacturaFecha = DateTime.UtcNow,
                FacturaSubtotal = subtotal,
                FacturaDescuento = 0,
                FacturaImpuesto = 0,
                FacturaTotal = subtotal,
                FacturaEstado = EstadoFactura.Emitida,
                FacturaFechaRegistro = DateTime.UtcNow
            };

            _context.Facturas.Add(factura);
            await _context.SaveChangesAsync();

            foreach (var d in detalles)
            {
                d.FacturaId = factura.FacturaId;
                _context.FacturaDetalle.Add(d);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Pago), new { id = factura.FacturaId });
        }

        // ── GET: /Consulta/Pago/5 ─────────────────────────────────────────────
        [Authorize(Roles = "Administrador,Medico,Cajero")]
        public async Task<IActionResult> Pago(int id)
        {
            var factura = await _context.Facturas
                .Include(f => f.Paciente)
                .Include(f => f.Detalles)
                .Include(f => f.Cita)
                .FirstOrDefaultAsync(f => f.FacturaId == id);

            if (factura == null) return NotFound();

            return View(factura);
        }

        // ── POST: /Consulta/RegistrarPago ─────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Medico,Cajero")]
        public async Task<IActionResult> RegistrarPago(int FacturaId)
        {
            var factura = await _context.Facturas
                .Include(f => f.Cita)
                .FirstOrDefaultAsync(f => f.FacturaId == FacturaId);

            if (factura == null) return NotFound();

            // Marcar factura como pagada
            factura.FacturaEstado = EstadoFactura.Pagada;

            // Marcar la cita como completada (usamos atendida que ya existe)
            if (factura.Cita != null)
                factura.Cita.EstadoCita = EstadoCita.atendida;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"Pago registrado. Factura {factura.FacturaNumero} completada.";

            return RedirectToAction(nameof(PagoCompletado),
                new { id = FacturaId });
        }

        // ── GET: /Consulta/PagoCompletado/5 ──────────────────────────────────
        [Authorize(Roles = "Administrador,Medico,Cajero")]
        public async Task<IActionResult> PagoCompletado(int id)
        {
            var factura = await _context.Facturas
                .Include(f => f.Paciente)
                .Include(f => f.Detalles)
                .Include(f => f.Cita)
                    .ThenInclude(c => c!.Medico)
                .FirstOrDefaultAsync(f => f.FacturaId == id);

            if (factura == null) return NotFound();

            return View(factura);
        }
    }
}