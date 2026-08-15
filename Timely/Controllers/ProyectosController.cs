using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Timely.Models;
using Timely.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Timely.Controllers
{
    [Authorize]
    public class ProyectosController : Controller
    {
        private readonly IProyectoService _proyectoService;
        private readonly IUsuarioService _usuarioService;

        public ProyectosController(IProyectoService proyectoService, IUsuarioService usuarioService)
        {
            _proyectoService = proyectoService;
            _usuarioService = usuarioService;
        }

        private int ObtenerUsuarioActualId()
        {
            var usuario = _usuarioService.ObtenerPerfil(User);
            return usuario?.Id ?? 0;
        }

        public ActionResult Tablero()
        {
            int usuarioId = ObtenerUsuarioActualId();

            if (usuarioId == 0) return RedirectToAction("Login", "Usuarios");

            var model = _proyectoService.ObtenerPorUsuario(usuarioId);
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
            proyecto.UsuarioId = ObtenerUsuarioActualId();

            try
            {
                // 2. Le dices al validador que ignore que estos campos no vinieron del HTML
                ModelState.Remove("Usuario");
                ModelState.Remove("UsuarioId");

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
            if (proyecto == null) return NotFound();

            if (proyecto.UsuarioId != ObtenerUsuarioActualId())
            {
                return Unauthorized();
            }

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
                if (proyecto.UsuarioId != ObtenerUsuarioActualId())
                {
                    return Unauthorized();
                }
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
            if (proyecto == null) return NotFound();

            if (proyecto.UsuarioId != ObtenerUsuarioActualId()) //En caso de que el proyecto no le pertenezca, se expulsa al usuario
            {
                return Unauthorized(); //Proximamente se redirigira a otra vista
            }

            return View(proyecto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Proyectos proyecto)
        {
            proyecto.UsuarioId = ObtenerUsuarioActualId();

            var proyectoOriginal = _proyectoService.BuscarPorId(proyecto.Id);
            if (proyectoOriginal.UsuarioId != proyecto.UsuarioId)
            {
                return Unauthorized();
            }

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

            proyecto.Completado = completado;
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
