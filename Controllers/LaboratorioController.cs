using CIVS.Data;
using CIVS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

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

        // ── GET: /Laboratorio ────────────────────────────────────────────────
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
                var texto = q.Trim();

                var palabras = texto.Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
                );

                foreach (var palabra in palabras)
                {
                    var termino = palabra;

                    query = query.Where(o =>
                        EF.Functions.Like(o.Examen.ExamenNombre ?? "", $"%{termino}%") ||
                        EF.Functions.Like(o.Consulta.Cita.Paciente.PacienteNombres ?? "", $"%{termino}%") ||
                        EF.Functions.Like(o.Consulta.Cita.Paciente.PacienteApellido ?? "", $"%{termino}%") ||
                        EF.Functions.Like(
                            ((o.Consulta.Cita.Paciente.PacienteNombres ?? "") + " " +
                             (o.Consulta.Cita.Paciente.PacienteApellido ?? "")),
                            $"%{termino}%") ||
                        EF.Functions.Like(o.Consulta.Cita.Paciente.PacienteDPI ?? "", $"%{termino}%")
                    );
                }
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
            try
            {
                orden.Consulta = null!;
                orden.Examen = null!;
                orden.OrdenDetalles = new List<OrdenExamenDetalle>();

                ModelState.Remove("Consulta");
                ModelState.Remove("Examen");
                ModelState.Remove("OrdenDetalles");

                if (orden.ConsultaId == 0)
                    ModelState.AddModelError("ConsultaId", "Debe seleccionar una consulta.");

                if (orden.ExamenId == 0)
                    ModelState.AddModelError("ExamenId", "Debe seleccionar un tipo de examen.");

                if (orden.ConsultaId > 0)
                {
                    var consultaExiste = await _context.Consultas
                        .AnyAsync(c => c.ConsultaId == orden.ConsultaId && c.ConsultaEstado);

                    if (!consultaExiste)
                        ModelState.AddModelError("ConsultaId", "La consulta seleccionada no existe o está inactiva.");
                }

                if (orden.ExamenId > 0)
                {
                    var examenExiste = await _context.Examenes
                        .AnyAsync(e => e.ExamenId == orden.ExamenId && e.ExamenEstado);

                    if (!examenExiste)
                        ModelState.AddModelError("ExamenId", "El examen seleccionado no existe o está inactivo.");
                }

                if (!ModelState.IsValid)
                {
                    await CargarCombos(orden.ConsultaId, orden.ExamenId);
                    TempData["Error"] = "Por favor corrige los errores en el formulario.";
                    return View(orden);
                }

                orden.OrdenEstado = EstadoOrdenExamen.Solicitado;
                orden.OrdenFecha = DateTime.Now;
                orden.ResultadoFecha = null;

                _context.OrdenExamenes.Add(orden);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Orden de examen #{orden.OrdenExamenId} creada exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                var mensajeError = ex.InnerException?.Message ?? ex.Message;
                TempData["Error"] = $"Error de base de datos: {mensajeError}";
                await CargarCombos(orden.ConsultaId, orden.ExamenId);
                return View(orden);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error inesperado: {ex.Message}";
                await CargarCombos(orden.ConsultaId, orden.ExamenId);
                return View(orden);
            }
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
                orden.ResultadoFecha = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Estado actualizado a: {estado}.";
            return RedirectToAction(nameof(Detalle), new { id });
        }

        // ── GET: /Laboratorio/CrearExamen ────────────────────────────────────
        public IActionResult CrearExamen()
        {
            return View();
        }

        // ── POST: /Laboratorio/CrearExamen ───────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearExamen(Examen examen)
        {
            bool nombreExiste = await _context.Examenes
                .AnyAsync(e => e.ExamenNombre == examen.ExamenNombre);

            if (nombreExiste)
            {
                ModelState.AddModelError(
                    nameof(examen.ExamenNombre),
                    "Ya existe un examen registrado con ese nombre.");
            }

            if (!ModelState.IsValid)
                return View(examen);

            examen.ExamenEstado = true;
            examen.ExamenFechaRegistro = DateTime.Now;

            _context.Examenes.Add(examen);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Examen '{examen.ExamenNombre}' registrado exitosamente.";
            return RedirectToAction(nameof(CrearExamen));
        }

        // ── GET: /Laboratorio/CrearReceta ────────────────────────────────────
        public async Task<IActionResult> CrearReceta()
        {
            await CargarConsultas();
            await CargarMedicamentosJson();
            return View();
        }

        // ── POST: /Laboratorio/CrearReceta ───────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearReceta(
            int ConsultaId,
            string? RecetaObservaciones,
            List<int?>? MedicamentoId,
            List<string?>? Dosis,
            List<string?>? Frecuencia,
            List<string?>? Duracion,
            List<string?>? Indicaciones)
        {
            MedicamentoId ??= new List<int?>();
            Dosis ??= new List<string?>();
            Frecuencia ??= new List<string?>();
            Duracion ??= new List<string?>();
            Indicaciones ??= new List<string?>();

            if (ConsultaId == 0)
            {
                ModelState.AddModelError("", "Debe seleccionar una consulta.");
                TempData["Error"] = "Debe seleccionar una consulta.";
                await CargarConsultas();
                await CargarMedicamentosJson();
                return View();
            }

            var consultaExiste = await _context.Consultas
                .AnyAsync(c => c.ConsultaId == ConsultaId && c.ConsultaEstado);

            if (!consultaExiste)
            {
                ModelState.AddModelError("", "La consulta seleccionada no existe o está inactiva.");
                TempData["Error"] = "La consulta seleccionada no existe o está inactiva.";
                await CargarConsultas(ConsultaId);
                await CargarMedicamentosJson();
                return View();
            }

            var medicamentosSeleccionados = MedicamentoId
                .Where(id => id.HasValue && id.Value > 0)
                .Select(id => id!.Value)
                .ToList();

            if (!medicamentosSeleccionados.Any())
            {
                ModelState.AddModelError("", "Debe agregar al menos un medicamento válido a la receta.");
                TempData["Error"] = "Debe agregar al menos un medicamento válido a la receta.";
                await CargarConsultas(ConsultaId);
                await CargarMedicamentosJson();
                return View();
            }

            var medicamentosExistentes = await _context.Medicamentos
                .Where(m => medicamentosSeleccionados.Contains(m.MedicamentoId) && m.MedicamentoEstado)
                .Select(m => m.MedicamentoId)
                .ToListAsync();

            var idsNoValidos = medicamentosSeleccionados
                .Where(id => !medicamentosExistentes.Contains(id))
                .ToList();

            if (idsNoValidos.Any())
            {
                ModelState.AddModelError("", "Uno o más medicamentos seleccionados no existen o están inactivos.");
                TempData["Error"] = "Uno o más medicamentos seleccionados no existen o están inactivos.";
                await CargarConsultas(ConsultaId);
                await CargarMedicamentosJson();
                return View();
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var receta = new Receta
                {
                    ConsultaId = ConsultaId,
                    RecetaObservaciones = RecetaObservaciones,
                    RecetaFechaEmision = DateTime.Now,
                    RecetaEstado = true
                };

                _context.Recetas.Add(receta);
                await _context.SaveChangesAsync();

                for (int i = 0; i < MedicamentoId.Count; i++)
                {
                    if (!MedicamentoId[i].HasValue || MedicamentoId[i].Value <= 0)
                        continue;

                    _context.RecetaDetalles.Add(new RecetaDetalle
                    {
                        RecetaId = receta.RecetaId,
                        MedicamentoId = MedicamentoId[i].Value,
                        Dosis = i < Dosis.Count ? Dosis[i] : null,
                        Frecuencia = i < Frecuencia.Count ? Frecuencia[i] : null,
                        Duracion = i < Duracion.Count ? Duracion[i] : null,
                        Indicaciones = i < Indicaciones.Count ? Indicaciones[i] : null
                    });
                }

                await _context.SaveChangesAsync();

                await CrearFacturaPendientePorRecetaAsync(receta.RecetaId);

                await transaction.CommitAsync();

                TempData["Success"] = "Receta médica creada exitosamente. También se generó una factura pendiente en Corte de Caja.";
                return RedirectToAction(nameof(CrearReceta));
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();

                var mensajeError = ex.InnerException?.Message ?? ex.Message;
                TempData["Error"] = $"Error de base de datos al crear la receta: {mensajeError}";

                await CargarConsultas(ConsultaId);
                await CargarMedicamentosJson();
                return View();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                TempData["Error"] = $"Error inesperado al crear la receta: {ex.Message}";

                await CargarConsultas(ConsultaId);
                await CargarMedicamentosJson();
                return View();
            }
        }

        // ── Helpers de combos ────────────────────────────────────────────────
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

            var consultasLista = consultas.Select(c => new
            {
                c.ConsultaId,
                Descripcion = $"{c.Cita.Paciente.PacienteNombres} {c.Cita.Paciente.PacienteApellido} — {c.ConsultaFechaRegistro:dd/MM/yyyy}"
            }).ToList();

            ViewBag.Consultas = new SelectList(consultasLista, "ConsultaId", "Descripcion", consultaId);
            ViewBag.Examenes = new SelectList(examenes, nameof(Examen.ExamenId), nameof(Examen.ExamenNombre), examenId);

            ViewBag.ConsultasJson = System.Text.Json.JsonSerializer.Serialize(
                consultasLista.Select(c => new
                {
                    consultaId = c.ConsultaId,
                    texto = c.Descripcion
                })
            );

            ViewBag.ExamenesJson = System.Text.Json.JsonSerializer.Serialize(
                examenes.Select(e => new
                {
                    examenId = e.ExamenId,
                    texto = e.ExamenNombre
                })
            );
        }

        private async Task CargarConsultas(int? seleccionado = null)
        {
            var consultas = await _context.Consultas
                .Include(c => c.Cita)
                    .ThenInclude(ci => ci.Paciente)
                .Where(c => c.ConsultaEstado)
                .OrderByDescending(c => c.ConsultaFechaRegistro)
                .ToListAsync();

            var consultasLista = consultas.Select(c => new
            {
                c.ConsultaId,
                Descripcion = $"{c.Cita.Paciente.PacienteNombres} {c.Cita.Paciente.PacienteApellido} — {c.ConsultaFechaRegistro:dd/MM/yyyy}"
            }).ToList();

            ViewBag.Consultas = new SelectList(consultasLista, "ConsultaId", "Descripcion", seleccionado);

            ViewBag.ConsultasJson = System.Text.Json.JsonSerializer.Serialize(
                consultasLista.Select(c => new
                {
                    consultaId = c.ConsultaId,
                    texto = c.Descripcion
                })
            );
        }

        private async Task CargarMedicamentosJson()
        {
            var medicamentos = await _context.Medicamentos
                .Where(m => m.MedicamentoEstado)
                .OrderBy(m => m.MedicamentoNombre)
                .Select(m => new
                {
                    medicamentoId = m.MedicamentoId,
                    medicamentoNombre = m.MedicamentoNombre,
                    medicamentoPresentacion = m.MedicamentoPresentacion,
                    medicamentoConcentracion = m.MedicamentoConcentracion,
                    medicamentoPrecio = m.MedicamentoPrecio ?? 0,
                    texto = m.MedicamentoNombre +
                            (m.MedicamentoPresentacion != null && m.MedicamentoPresentacion != "" ? " — " + m.MedicamentoPresentacion : "") +
                            (m.MedicamentoConcentracion != null && m.MedicamentoConcentracion != "" ? " " + m.MedicamentoConcentracion : "")
                })
                .ToListAsync();

            ViewBag.MedicamentosJson = System.Text.Json.JsonSerializer.Serialize(medicamentos);
        }


        private async Task CrearFacturaPendientePorRecetaAsync(int recetaId)
        {
            var receta = await _context.Recetas
                .Include(r => r.Consulta)
                    .ThenInclude(c => c.Cita)
                        .ThenInclude(ci => ci.Paciente)
                .Include(r => r.RecetaDetalles)
                    .ThenInclude(rd => rd.Medicamento)
                .FirstOrDefaultAsync(r => r.RecetaId == recetaId);

            if (receta == null || receta.Consulta?.Cita == null)
                return;

            var citaId = receta.Consulta.CitaId;

            var factura = await _context.Facturas
                .FirstOrDefaultAsync(f =>
                    f.CitaId == citaId &&
                    f.FacturaEstado == EstadoFactura.Emitida);

            if (factura == null)
            {
                int correlativo = await _context.Facturas.CountAsync() + 1;

                factura = new Factura
                {
                    FacturaNumero = $"FAC-{correlativo:D6}",
                    PacienteId = receta.Consulta.Cita.PacienteId,
                    CitaId = citaId,
                    FacturaFecha = DateTime.Now,
                    FacturaSubtotal = 0,
                    FacturaDescuento = 0,
                    FacturaImpuesto = 0,
                    FacturaTotal = 0,
                    FacturaEstado = EstadoFactura.Emitida,
                    FacturaFechaRegistro = DateTime.Now
                };

                _context.Facturas.Add(factura);
                await _context.SaveChangesAsync();
            }

            decimal subtotalAgregado = 0;

            foreach (var detalle in receta.RecetaDetalles)
            {
                var medicamento = detalle.Medicamento;

                if (medicamento == null)
                    continue;

                decimal precio = medicamento.MedicamentoPrecio ?? 0;
                int cantidad = 1;
                decimal descuento = 0;
                decimal totalLinea = (cantidad * precio) - descuento;

                if (totalLinea < 0)
                    totalLinea = 0;

                subtotalAgregado += totalLinea;

                _context.FacturaDetalle.Add(new FacturaDetalle
                {
                    FacturaId = factura.FacturaId,
                    MedicamentoId = medicamento.MedicamentoId,
                    DetalleDescripcion = $"{medicamento.MedicamentoNombre}" +
                                         $"{(!string.IsNullOrWhiteSpace(medicamento.MedicamentoPresentacion) ? " — " + medicamento.MedicamentoPresentacion : "")}" +
                                         $"{(!string.IsNullOrWhiteSpace(medicamento.MedicamentoConcentracion) ? " " + medicamento.MedicamentoConcentracion : "")}" +
                                         $"{(!string.IsNullOrWhiteSpace(detalle.Dosis) ? " — " + detalle.Dosis : "")}",
                    DetalleCantidad = cantidad,
                    DetallePrecioUnitario = precio,
                    DetalleDescuento = descuento,
                    DetalleTotalLinea = totalLinea
                });
            }

            factura.FacturaSubtotal += subtotalAgregado;
            factura.FacturaTotal = (factura.FacturaSubtotal - factura.FacturaDescuento) + factura.FacturaImpuesto;

            if (factura.FacturaTotal < 0)
                factura.FacturaTotal = 0;

            await _context.SaveChangesAsync();
        }
    }
}