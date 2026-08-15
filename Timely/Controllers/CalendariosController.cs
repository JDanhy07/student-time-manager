using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Timely.Models;
using Timely.Services.Interfaces;

namespace Timely.Controllers
{
    [Authorize]
    public class CalendariosController : Controller
    {
        private readonly ICalendarioService _calendarioService;
        private readonly IUsuarioService _usuarioService;

        public CalendariosController(ICalendarioService calendarioService, IUsuarioService usuarioService)
        {
            _calendarioService = calendarioService;
            _usuarioService = usuarioService;
        }

        private int ObtenerUsuarioActualId()
        {
            var usuario = _usuarioService.ObtenerPerfil(User);
            return usuario?.Id ?? 0;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ObtenerEventos()
        {
            int usuarioId = ObtenerUsuarioActualId();
            var misEventos = _calendarioService.ObtenerEventosPorUsuario(usuarioId);

            var eventosFormateados = misEventos.Select(e => new
            {
                id = e.Id,
                title = e.Titulo,
                start = e.FechaInicio,
                end = e.FechaFinal,
                description = e.Descripcion
            });

            return Json(eventosFormateados);
        }

        [HttpPost]
        public IActionResult CrearEvento([FromBody] Calendario evento)
        {
            evento.UsuarioId = ObtenerUsuarioActualId();

            ModelState.Remove("Usuario");
            ModelState.Remove("UsuarioId");

            if (ModelState.IsValid)
            {
                _calendarioService.AgregarEvento(evento);
                return Json(new { success = true, message = "Evento creado correctamente" });
            }

            return BadRequest(new { success = false, message = "Datos incompletos" });
        }

        [HttpPost]
        public IActionResult EditarEvento([FromBody] Calendario evento)
        {
            evento.UsuarioId = ObtenerUsuarioActualId();

            var eventoOriginal = _calendarioService.ObtenerEventoPorId(evento.Id);
            if (eventoOriginal == null) return NotFound(new { success = false, message = "Evento no encontrado" });

            if (eventoOriginal.UsuarioId != evento.UsuarioId)
            {
                return Unauthorized(new { success = false, message = "No tienes permiso para editar este evento" });
            }

            _calendarioService.EditarEvento(evento);
            return Json(new { success = true, message = "Evento actualizado" });
        }

        [HttpPost]
        public IActionResult EliminarEvento([FromBody] int id)
        {
            var evento = _calendarioService.ObtenerEventoPorId(id);
            if (evento == null) return NotFound(new { success = false, message = "Evento no encontrado" });

            if (evento.UsuarioId != ObtenerUsuarioActualId())
            {
                return Unauthorized(new { success = false, message = "No tienes permiso para eliminar este evento" });
            }

            _calendarioService.EliminarEvento(id);
            return Json(new { success = true, message = "Evento eliminado" });
        }
    }
}