using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Timely.Models;
using Timely.Services.Interfaces;

namespace Timely.Controllers
{
    public class ProyectosController : Controller
    {
        private readonly IProyectoService _proyectoService;

        public ProyectosController(IProyectoService proyectoService)
        {
            _proyectoService = proyectoService;
        }

        public ActionResult Tablero()
        {
            var model = _proyectoService.ObtenerTodos();
            return View(model);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Proyectos proyecto)
        {
            try
            {
                if (ModelState.IsValid)
                _proyectoService.AgregarProyecto(proyecto);
                return RedirectToAction(nameof(Tablero));
            }
            catch
            {
                ModelState.AddModelError("", "El proyecto que deseas registrar ya está agendado. Intenta con otro.");
                return View(proyecto);
            }
        }

        // Antes: GET que hacía el borrado directamente — grave error de seguridad
        // Ahora: GET solo muestra la vista de confirmación
        public ActionResult Delete(int id)
        {
            var proyecto = _proyectoService.BuscarPorId(id);
            return View(proyecto);
        }

        // El borrado real solo ocurre desde POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmado(int id)
        {
            try
            {
                var proyecto = _proyectoService.BuscarPorId(id);
                _proyectoService.EliminarProyecto(proyecto);
                return RedirectToAction(nameof(Tablero));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction(nameof(Tablero));
            }
        }

        public ActionResult Edit(int id)
        {
            var proyecto = _proyectoService.BuscarPorId(id);
            return View(proyecto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Proyectos proyecto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _proyectoService.ActualizarProyecto(proyecto);
                    return RedirectToAction(nameof(Tablero));
                }
                return View(proyecto);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(proyecto);
            }
        }

        // Antes: accedía a Service.Proyectos directamente desde el controller
        // Ahora: delega al servicio
        [HttpGet("vencidos")]
        public async Task<IActionResult> GetProyectosVencidos()
        {
            var vencidos = await _proyectoService.ObtenerVencidosAsync();
            return Ok(vencidos);
        }

        [HttpPost]
        public ActionResult ActualizarEstado([FromBody] JsonElement data)
        {
            int id = data.GetProperty("id").GetInt32();
            bool completado = data.GetProperty("completado").GetBoolean();

            var proyecto = _proyectoService.BuscarPorId(id);

            proyecto.Estado = completado ? "Hecho" : ResolverEstadoPorFecha(proyecto);
            _proyectoService.ActualizarProyecto(proyecto);

            return Json(new
            {
                nuevoEstado = proyecto.Estado,
                nuevaClase = _proyectoService.ObtenerClaseEstado(proyecto.Fecha_de_inicio, proyecto.Vence, proyecto.Estado)
            });
        }

        // Método privado auxiliar — solo orquesta, no es lógica de negocio pesada
        private static string ResolverEstadoPorFecha(Proyectos proyecto)
        {
            DateTime hoy = DateTime.Now;
            if (hoy < proyecto.Fecha_de_inicio) return "Inactivo";
            if (hoy > proyecto.Vence) return "Vencido";
            return "En proceso";
        }
    }
}
