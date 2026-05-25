
using CIVS.Data;
using CIVS.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CIVS.Controllers
{
    [Authorize(Roles = "Administrador,Recepcionista,Cajero,Laboratorista")]
    // Se crea el controllador que hereda de la clase Controller de ASP.NET
    public class MedicamentoController : Controller
    {
        // Se crea un campo privado que solo en esta clase se puede acceder y cuando se le asigna un valor ya no se puede cambiar (readonly
        private readonly AppDbContext _context;

        //Crea el constructor de la clase y toma como parametro AppDbContext significa que ASP.NET Core le está enviando el contexto de base de datos
        public MedicamentoController(AppDbContext context)
        {
            _context = context;
        }

        //GET: /Medicamento --> Trae los medicamentos de la dB
        // Clase que trabaja de forma asincronica (async) y que devolverá una respuesta MVC (TASK<IActionResult>)
        public async Task<IActionResult> Index(string? q)
        {
            var query = _context.Medicamentos.AsQueryable(); //Aqui guarda la representacion de la tabla medicamentos
            if(!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(m =>
                m.MedicamentoNombre.Contains(q) ||
                (m.MedicamentoPresentacion != null && m.MedicamentoPresentacion.Contains(q)) ||
                (m.MedicamentoConcentracion != null && m.MedicamentoConcentracion.Contains(q)) ||
                (m.MedicamentoUnidad != null && m.MedicamentoUnidad.Contains(q))
                );
            }

            // Variable que espera a que la consulta termine, en orden descendente y convierte el resultado en una lista y lo hace de forma asíncrona.
            var medicamentos = await query 
                .OrderByDescending(m => m.MedicamentoFechaRegistro)
                .ToListAsync();

            // Aquí guardas el valor de búsqueda y se envia ese valor a la vista.
            ViewBag.Q = q;
            return View(medicamentos);
        }

        //GET: /Medicamento/CrearMedicamentoInsumo
        public IActionResult CrearMedicamentoInsumo()
        {
            return View();
        }

        //POST: /Medicamento/CrearMedicamentoInsumo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearMedicamentoInsumo(Medicamento medicamento)
        {
            bool nombreExiste = await _context.Medicamentos
                .AnyAsync(m => m.MedicamentoNombre == medicamento.MedicamentoNombre);

            if (nombreExiste)
                ModelState.AddModelError(nameof(medicamento.MedicamentoNombre),
                    "Ya existe un medicamento o insumo registrado con ese nombre");

            if(!ModelState.IsValid) 
                return View(medicamento);

            medicamento.MedicamentoEstado = true;
            medicamento.MedicamentoFechaRegistro = DateTime.UtcNow;

            _context.Medicamentos.Add(medicamento);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"{medicamento.MedicamentoNombre} registrado exitosamente.";
            return RedirectToAction(nameof(CrearMedicamentoInsumo));

        }


        // GET: EditarMedicamentoInsumo/5
        public async Task<IActionResult> EditarMedicamentoInsumo(int id)
        {
            var medicamento = await _context.Medicamentos.FindAsync(id);

            if (medicamento == null)
                return NotFound();

            return View(medicamento); // Views/TuControlador/EditarMedicamentoInsumo.cshtml
        }

        // POST: EditarMedicamentoInsumo/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarMedicamentoInsumo(int id, Medicamento datos)
        {
            var medicamento = await _context.Medicamentos.FindAsync(id);

            if (medicamento == null)
                return NotFound();

            if (!ModelState.IsValid)
                return View(datos);

            medicamento.MedicamentoNombre = datos.MedicamentoNombre;
            medicamento.MedicamentoPresentacion = datos.MedicamentoPresentacion;
            medicamento.MedicamentoConcentracion = datos.MedicamentoConcentracion;
            medicamento.MedicamentoUnidad = datos.MedicamentoUnidad;
            medicamento.MedicamentoPrecio = datos.MedicamentoPrecio;

            await RegistrarAuditoriaAsync(
                "UPDATE",
                "Medicamento",
                medicamento.MedicamentoId.ToString(),
                $"Editó el medicamento/insumo: {medicamento.MedicamentoNombre}."
            );

            await _context.SaveChangesAsync();

            TempData["Success"] = "Medicamento actualizado exitosamente.";
            return RedirectToAction(nameof(Index), new { q = medicamento.MedicamentoNombre });
        }

        // POST: ActivarDesactivarMedicamento/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivarDesactivarMedicamento(int id)
        {
            var medicamento = await _context.Medicamentos.FindAsync(id);

            if (medicamento == null)
                return NotFound();

            medicamento.MedicamentoEstado = !medicamento.MedicamentoEstado;

            string accion = medicamento.MedicamentoEstado ? "ACTIVATE" : "DEACTIVATE";
            string estadoTexto = medicamento.MedicamentoEstado ? "activó" : "desactivó";

            await RegistrarAuditoriaAsync(
                accion,
                "Medicamento",
                medicamento.MedicamentoId.ToString(),
                $"El usuario {estadoTexto} el medicamento/insumo: {medicamento.MedicamentoNombre}."
            );

            await _context.SaveChangesAsync();

            TempData["Success"] = medicamento.MedicamentoEstado
                ? "Medicamento activado exitosamente."
                : "Medicamento desactivado exitosamente.";

            return RedirectToAction(nameof(Index), new { q = medicamento.MedicamentoNombre });
        }

        // ── Helpers de auditoría ────────────────────────────────────────────────
        private async Task RegistrarAuditoriaAsync(string accion, string entidad, string entidadId, string descripcion)
        {
            var usuarioId = await ObtenerUsuarioActualIdAsync();

            _context.Auditorias.Add(new Auditoria
            {
                UsuarioId = usuarioId,
                AuditoriaAccion = accion,
                AuditoriaEntidad = entidad,
                AuditoriaEntidadId = entidadId,
                AuditoriaDescripcion = descripcion,
                AuditoriaFecha = DateTime.Now
            });
        }
        
        private async Task<int?> ObtenerUsuarioActualIdAsync()
        {
            var username = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(username))
            {
                username =
                    User.FindFirst(ClaimTypes.Name)?.Value ??
                    User.FindFirst(ClaimTypes.Email)?.Value ??
                    User.FindFirst("UsuarioUsername")?.Value;
            }

            if (string.IsNullOrWhiteSpace(username))
                return null;

            return await _context.Usuarios
                .Where(u => u.UsuarioUsername == username || u.UsuarioEmail == username)
                .Select(u => (int?)u.UsuarioId)
                .FirstOrDefaultAsync();
        }

    }
}
