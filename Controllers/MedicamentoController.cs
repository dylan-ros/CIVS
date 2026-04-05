
using CIVS.Data;
using CIVS.Models;
using Microsoft.AspNetCore.Authorization;
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

    }
}
