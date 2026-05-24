using CIVS.Data;
using CIVS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CIVS.Controllers
{
    [Authorize(Roles = "Administrador,Contabilidad")]
    public class CorteCajaController : Controller
    {
        private readonly AppDbContext _context;

        public CorteCajaController(AppDbContext context)
        {
            _context = context;
        }

        // ── GET: /CorteCaja (Reporte financiero / corte del día) ─────────────
        public async Task<IActionResult> Index(DateTime? fecha)
        {
            var fechaFiltro = fecha?.Date ?? DateTime.Now.Date;

            // Pagos realizados en la fecha seleccionada
            var pagosDelDia = await _context.Pagos
                .Include(p => p.MetodoPago)
                .Include(p => p.Factura)
                    .ThenInclude(f => f.Paciente)
                .Include(p => p.Factura)
                    .ThenInclude(f => f.Detalles)
                .Where(p => p.PagoEstado
                         && p.PagoFecha.Date == fechaFiltro
                         && p.Factura.FacturaEstado != EstadoFactura.Anulada)
                .OrderByDescending(p => p.PagoFecha)
                .ToListAsync();

            // Facturas pagadas ese día, sin duplicar
            var facturasPagadasDelDia = pagosDelDia
                .Select(p => p.Factura)
                .GroupBy(f => f.FacturaId)
                .Select(g => g.First())
                .OrderByDescending(f => f.FacturaFecha)
                .ToList();

            // Todas las facturas pendientes actuales
            var facturasPendientes = await _context.Facturas
                .Include(f => f.Paciente)
                .Where(f => f.FacturaEstado == EstadoFactura.Emitida)
                .ToListAsync();

            ViewBag.Fecha = fechaFiltro.ToString("yyyy-MM-dd");
            ViewBag.FechaDisplay = fechaFiltro.ToString("dd/MM/yyyy");

            // Total real cobrado en caja
            ViewBag.TotalDia = pagosDelDia.Sum(p => p.PagoMonto);

            // Pendiente actual de cobro
            ViewBag.TotalPendiente = facturasPendientes.Sum(f => f.FacturaTotal);

            ViewBag.TotalFacturas = facturasPagadasDelDia.Count;
            ViewBag.TotalPagadas = facturasPagadasDelDia.Count;
            ViewBag.TotalEmitidas = facturasPendientes.Count;

            // Totales por método de pago
            ViewBag.PorMetodoPago = pagosDelDia
                .GroupBy(p => p.MetodoPago.MetodoPagoNombre)
                .Select(g => new
                {
                    Metodo = g.Key,
                    Total = g.Sum(p => p.PagoMonto)
                })
                .ToList();

            var hoy = DateTime.Now.Date;

            var inicioSemana = hoy.AddDays(-6);
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
            var inicioAnio = new DateTime(hoy.Year, 1, 1);
            var finManana = hoy.AddDays(1);

            // Pagos últimos 7 días
            var pagosSemana = await _context.Pagos
                .Where(p => p.PagoEstado
                         && p.PagoFecha >= inicioSemana
                         && p.PagoFecha < finManana
                         && p.Factura.FacturaEstado != EstadoFactura.Anulada)
                .ToListAsync();

            var labelsSemana = Enumerable.Range(0, 7)
                .Select(i => inicioSemana.AddDays(i))
                .ToList();

            ViewBag.LabelsSemana = JsonSerializer.Serialize(
                labelsSemana.Select(d => d.ToString("dd/MM")).ToList());

            ViewBag.DataSemana = JsonSerializer.Serialize(
                labelsSemana.Select(d =>
                    pagosSemana
                        .Where(p => p.PagoFecha.Date == d.Date)
                        .Sum(p => p.PagoMonto)
                ).ToList());

            // Pagos del mes actual
            var pagosMes = await _context.Pagos
                .Where(p => p.PagoEstado
                         && p.PagoFecha >= inicioMes
                         && p.PagoFecha < finManana
                         && p.Factura.FacturaEstado != EstadoFactura.Anulada)
                .ToListAsync();

            var diasMes = Enumerable.Range(1, hoy.Day)
                .Select(d => new DateTime(hoy.Year, hoy.Month, d))
                .ToList();

            ViewBag.LabelsMes = JsonSerializer.Serialize(
                diasMes.Select(d => d.Day.ToString()).ToList());

            ViewBag.DataMes = JsonSerializer.Serialize(
                diasMes.Select(d =>
                    pagosMes
                        .Where(p => p.PagoFecha.Date == d.Date)
                        .Sum(p => p.PagoMonto)
                ).ToList());

            // Pagos del año actual
            var pagosAnio = await _context.Pagos
                .Where(p => p.PagoEstado
                         && p.PagoFecha >= inicioAnio
                         && p.PagoFecha < finManana
                         && p.Factura.FacturaEstado != EstadoFactura.Anulada)
                .ToListAsync();

            var meses = Enumerable.Range(1, 12).ToList();
            var nombresMeses = new[]
            {
                "Ene", "Feb", "Mar", "Abr", "May", "Jun",
                "Jul", "Ago", "Sep", "Oct", "Nov", "Dic"
            };

            ViewBag.LabelsAnio = JsonSerializer.Serialize(nombresMeses);

            ViewBag.DataAnio = JsonSerializer.Serialize(
                meses.Select(m =>
                    pagosAnio
                        .Where(p => p.PagoFecha.Month == m)
                        .Sum(p => p.PagoMonto)
                ).ToList());

            ViewBag.TotalSemana = pagosSemana.Sum(p => p.PagoMonto);
            ViewBag.TotalMes = pagosMes.Sum(p => p.PagoMonto);
            ViewBag.TotalAnio = pagosAnio.Sum(p => p.PagoMonto);

            return View(facturasPagadasDelDia);
        }

        // ── GET: /CorteCaja/CrearFactura ─────────────────────────────────────
        public async Task<IActionResult> CrearFactura()
        {
            await CargarCombos();
            return View();
        }

        // ── POST: /CorteCaja/CrearFactura ────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearFactura(
            int PacienteId,
            int? CitaId,
            int MetodoPagoId,
            string? PagoReferencia,
            List<int?> MedicamentoId,
            List<string> DetalleDescripcion,
            List<int> DetalleCantidad,
            List<decimal> DetallePrecioUnitario,
            List<decimal> DetalleDescuento)
        {
            if (PacienteId == 0)
            {
                ModelState.AddModelError("", "Debe seleccionar un paciente.");
                await CargarCombos();
                return View();
            }

            if (DetalleDescripcion == null || !DetalleDescripcion.Any(d => !string.IsNullOrWhiteSpace(d)))
            {
                ModelState.AddModelError("", "Debe agregar al menos un servicio a la factura.");
                await CargarCombos(PacienteId, CitaId, MetodoPagoId);
                return View();
            }

            // Calcular totales
            decimal subtotal = 0;
            var detalles = new List<FacturaDetalle>();

            for (int i = 0; i < DetalleDescripcion.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(DetalleDescripcion[i]))
                    continue;

                int cantidad = i < DetalleCantidad.Count ? DetalleCantidad[i] : 1;
                decimal precio = i < DetallePrecioUnitario.Count ? DetallePrecioUnitario[i] : 0;
                decimal descuento = i < DetalleDescuento.Count ? DetalleDescuento[i] : 0;

                int? medicamentoId = null;

                if (MedicamentoId != null &&
                    i < MedicamentoId.Count &&
                    MedicamentoId[i].HasValue &&
                    MedicamentoId[i].Value > 0)
                {
                    medicamentoId = MedicamentoId[i].Value;

                    var medicamento = await _context.Medicamentos
                        .AsNoTracking()
                        .FirstOrDefaultAsync(m => m.MedicamentoId == medicamentoId.Value);

                    if (medicamento != null)
                    {
                        precio = medicamento.MedicamentoPrecio ?? 0;

                        DetalleDescripcion[i] =
                            $"{medicamento.MedicamentoNombre}" +
                            $"{(!string.IsNullOrWhiteSpace(medicamento.MedicamentoPresentacion) ? " — " + medicamento.MedicamentoPresentacion : "")}" +
                            $"{(!string.IsNullOrWhiteSpace(medicamento.MedicamentoConcentracion) ? " " + medicamento.MedicamentoConcentracion : "")}";
                    }
                }

                decimal totalLinea = (cantidad * precio) - descuento;

                if (totalLinea < 0)
                    totalLinea = 0;

                subtotal += totalLinea;

                detalles.Add(new FacturaDetalle
                {
                    MedicamentoId = medicamentoId,
                    DetalleDescripcion = DetalleDescripcion[i],
                    DetalleCantidad = cantidad,
                    DetallePrecioUnitario = precio,
                    DetalleDescuento = descuento,
                    DetalleTotalLinea = totalLinea
                });
            }

            // Correlativo automático
            int ultimoId = await _context.Facturas.CountAsync() + 1;

            var factura = new Factura
            {
                FacturaNumero = $"FAC-{ultimoId:D6}",
                PacienteId = PacienteId,
                CitaId = CitaId == 0 ? null : CitaId,
                FacturaFecha = DateTime.Now,
                FacturaSubtotal = subtotal,
                FacturaDescuento = 0,
                FacturaImpuesto = 0,
                FacturaTotal = subtotal,
                FacturaEstado = EstadoFactura.Pagada,
                FacturaFechaRegistro = DateTime.Now
            };

            _context.Facturas.Add(factura);
            await _context.SaveChangesAsync();

            // Guardar detalles
            foreach (var d in detalles)
            {
                d.FacturaId = factura.FacturaId;
                _context.FacturaDetalle.Add(d);
            }

            // Registrar pago
            _context.Pagos.Add(new Pago
            {
                FacturaId = factura.FacturaId,
                MetodoPagoId = MetodoPagoId,
                PagoMonto = subtotal,
                PagoReferencia = PagoReferencia,
                PagoFecha = DateTime.Now,
                PagoEstado = true
            });

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Factura {factura.FacturaNumero} creada exitosamente. Total: Q {subtotal:0.00}";
            return RedirectToAction(nameof(CrearFactura));
        }

        // ── GET: /CorteCaja/DetalleFactura/5 ─────────────────────────────────
        public async Task<IActionResult> DetalleFactura(int id)
        {
            var factura = await _context.Facturas
                .Include(f => f.Paciente)
                .Include(f => f.Cita)
                    .ThenInclude(c => c != null ? c.Medico : null)
                .Include(f => f.Detalles)
                .Include(f => f.Pagos)
                    .ThenInclude(p => p.MetodoPago)
                .FirstOrDefaultAsync(f => f.FacturaId == id);

            if (factura == null)
                return NotFound();

            return View(factura);
        }

        // ── Helper ────────────────────────────────────────────────────────────
        private async Task CargarCombos(
            int? pacienteId = null,
            int? citaId = null,
            int? metodoPagoId = null)
        {
            var pacientes = await _context.Pacientes
                .Where(p => p.PacienteEstado)
                .OrderBy(p => p.PacienteNombres)
                .ToListAsync();

            var citas = await _context.Citas
                .Include(c => c.Paciente)
                .Where(c => c.EstadoCita == EstadoCita.atendida)
                .OrderByDescending(c => c.CitaFechaInicio)
                .ToListAsync();

            var metodos = await _context.MetodoPagos
                .Where(m => m.MetodoPagoEstado)
                .OrderBy(m => m.MetodoPagoNombre)
                .ToListAsync();

            var pacientesLista = pacientes.Select(p => new
            {
                p.PacienteId,
                Nombre = $"{p.PacienteNombres} {p.PacienteApellido}",
                Dpi = p.PacienteDPI ?? ""
            }).ToList();

            var citasLista = citas.Select(c => new
            {
                c.CitaId,
                c.PacienteId,
                Descripcion = $"{c.Paciente.PacienteNombres} {c.Paciente.PacienteApellido} — {c.CitaFechaInicio:dd/MM/yyyy HH:mm}",
                PacienteNombre = $"{c.Paciente.PacienteNombres} {c.Paciente.PacienteApellido}"
            }).ToList();

            // Se dejan por compatibilidad con otras vistas o validaciones existentes.
            ViewBag.Pacientes = new SelectList(
                pacientesLista,
                "PacienteId",
                "Nombre",
                pacienteId);

            ViewBag.Citas = new SelectList(
                citasLista,
                "CitaId",
                "Descripcion",
                citaId);

            ViewBag.MetodosPago = new SelectList(
                metodos,
                nameof(MetodoPago.MetodoPagoId),
                nameof(MetodoPago.MetodoPagoNombre),
                metodoPagoId);

            // JSON para buscadores.
            ViewBag.PacientesJson = System.Text.Json.JsonSerializer.Serialize(
                pacientesLista.Select(p => new
                {
                    pacienteId = p.PacienteId,
                    texto = string.IsNullOrWhiteSpace(p.Dpi)
                        ? p.Nombre
                        : $"{p.Nombre} — DPI: {p.Dpi}"
                })
            );

            ViewBag.CitasJson = System.Text.Json.JsonSerializer.Serialize(
                citasLista.Select(c => new
                {
                    citaId = c.CitaId,
                    pacienteId = c.PacienteId,
                    pacienteNombre = c.PacienteNombre,
                    texto = c.Descripcion
                })
            );
        }


        // GET: /CorteCaja/FacturasPendientes
        public async Task<IActionResult> FacturasPendientes()
        {
            var facturas = await _context.Facturas
                .Include(f => f.Paciente)
                .Include(f => f.Cita)
                    .ThenInclude(c => c!.Medico)
                .Include(f => f.Detalles)
                .Where(f => f.FacturaEstado == EstadoFactura.Emitida)
                .OrderBy(f => f.FacturaFecha)
                .ToListAsync();

            return View(facturas);
        }

        // POST: /CorteCaja/MarcarFacturaPagada
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarFacturaPagada(int facturaId)
        {
            var factura = await _context.Facturas
                .Include(f => f.Pagos)
                .FirstOrDefaultAsync(f => f.FacturaId == facturaId);

            if (factura == null)
                return NotFound();

            if (factura.FacturaEstado == EstadoFactura.Pagada)
            {
                TempData["Error"] = "Esta factura ya está marcada como pagada.";
                return RedirectToAction(nameof(FacturasPendientes));
            }

            var metodoPago = await _context.MetodoPagos
                .Where(m => m.MetodoPagoEstado)
                .OrderBy(m => m.MetodoPagoId)
                .FirstOrDefaultAsync();

            if (metodoPago == null)
            {
                TempData["Error"] = "No hay métodos de pago activos. Debe registrar al menos uno.";
                return RedirectToAction(nameof(FacturasPendientes));
            }

            var fechaPago = DateTime.Now;

            factura.FacturaEstado = EstadoFactura.Pagada;

            // Esto ayuda a que en detalle y reportes se vea como cobrada hoy
            factura.FacturaFecha = fechaPago;

            bool yaTienePago = factura.Pagos != null && factura.Pagos.Any(p => p.PagoEstado);

            if (!yaTienePago)
            {
                _context.Pagos.Add(new Pago
                {
                    FacturaId = factura.FacturaId,
                    MetodoPagoId = metodoPago.MetodoPagoId,
                    PagoMonto = factura.FacturaTotal,
                    PagoReferencia = "Pago registrado desde facturas pendientes",
                    PagoFecha = fechaPago,
                    PagoEstado = true
                });
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Factura {factura.FacturaNumero} marcada como pagada y reflejada en el corte de caja.";

            return RedirectToAction(nameof(Index), new
            {
                fecha = fechaPago.ToString("yyyy-MM-dd")
            });
        }

        // ── GET: /CorteCaja/Facturas ─────────────────────────────────────
        public async Task<IActionResult> Facturas(string? numero)
        {
            var query = _context.Facturas
                .Include(f => f.Paciente)
                .Include(f => f.Pagos)
                    .ThenInclude(p => p.MetodoPago)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(numero))
            {
                numero = numero.Trim();

                query = query.Where(f =>
                    f.FacturaNumero != null &&
                    f.FacturaNumero.Contains(numero));
            }

            var facturas = await query
                .OrderByDescending(f => f.FacturaFecha)
                .Take(100)
                .ToListAsync();

            ViewBag.Numero = numero;
            ViewBag.TotalFacturas = facturas.Count;

            return View(facturas);
        }


        // ── GET: /CorteCaja/FacturaImprimir/5 ─────────────────────────────
        public async Task<IActionResult> FacturaImprimir(int id)
        {
            var factura = await _context.Facturas
                .Include(f => f.Paciente)
                .Include(f => f.Cita)
                    .ThenInclude(c => c!.Medico)
                .Include(f => f.Detalles)
                .Include(f => f.Pagos)
                    .ThenInclude(p => p.MetodoPago)
                .FirstOrDefaultAsync(f => f.FacturaId == id);

            if (factura == null)
                return NotFound();

            return View(factura);
        }




    }
}