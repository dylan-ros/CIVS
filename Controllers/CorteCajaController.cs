using CIVS.Data;
using CIVS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CIVS.Controllers
{
    [Authorize(Roles = "Administrador,Cajero")]
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
            var fechaFiltro = fecha?.Date ?? DateTime.UtcNow.Date;

            var facturas = await _context.Facturas
                .Include(f => f.Paciente)
                .Include(f => f.Pagos)
                    .ThenInclude(p => p.MetodoPago)
                .Where(f => f.FacturaFecha.Date == fechaFiltro
                         && f.FacturaEstado != EstadoFactura.Anulada)
                .OrderByDescending(f => f.FacturaFecha)
                .ToListAsync();

            ViewBag.Fecha = fechaFiltro.ToString("yyyy-MM-dd");
            ViewBag.FechaDisplay = fechaFiltro.ToString("dd/MM/yyyy");
            ViewBag.TotalDia = facturas.Sum(f => f.FacturaTotal);
            ViewBag.TotalFacturas = facturas.Count;
            ViewBag.TotalPagadas = facturas.Count(f => f.FacturaEstado == EstadoFactura.Pagada);
            ViewBag.TotalEmitidas = facturas.Count(f => f.FacturaEstado == EstadoFactura.Emitida);

            // Totales por método de pago
            ViewBag.PorMetodoPago = facturas
                .SelectMany(f => f.Pagos)
                .GroupBy(p => p.MetodoPago.MetodoPagoNombre)
                .Select(g => new { Metodo = g.Key, Total = g.Sum(p => p.PagoMonto) })
                .ToList();

            return View(facturas);
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
                if (string.IsNullOrWhiteSpace(DetalleDescripcion[i])) continue;

                decimal cantidad = DetalleCantidad[i];
                decimal precio = DetallePrecioUnitario[i];
                decimal descuento = DetalleDescuento[i];
                decimal totalLinea = (cantidad * precio) - descuento;

                subtotal += totalLinea;

                detalles.Add(new FacturaDetalle
                {
                    DetalleDescripcion = DetalleDescripcion[i],
                    DetalleCantidad = DetalleCantidad[i],
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
                FacturaFecha = DateTime.UtcNow,
                FacturaSubtotal = subtotal,
                FacturaDescuento = 0,
                FacturaImpuesto = 0,
                FacturaTotal = subtotal,
                FacturaEstado = EstadoFactura.Pagada,
                FacturaFechaRegistro = DateTime.UtcNow
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
                PagoFecha = DateTime.UtcNow,
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

            ViewBag.Pacientes = new SelectList(
                pacientes.Select(p => new {
                    p.PacienteId,
                    Nombre = $"{p.PacienteNombres} {p.PacienteApellido}"
                }),
                "PacienteId", "Nombre", pacienteId);

            ViewBag.Citas = new SelectList(
                citas.Select(c => new {
                    c.CitaId,
                    Descripcion = $"{c.Paciente.PacienteNombres} {c.Paciente.PacienteApellido} — {c.CitaFechaInicio:dd/MM/yyyy}"
                }),
                "CitaId", "Descripcion", citaId);

            ViewBag.MetodosPago = new SelectList(
                metodos, nameof(MetodoPago.MetodoPagoId),
                nameof(MetodoPago.MetodoPagoNombre), metodoPagoId);
        }
    }
}