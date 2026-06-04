using Microsoft.AspNetCore.Mvc;
using Timely.Models;
using Timely.Services.Interfaces;

namespace Timely.Controllers
{
    public class CalendariosController : Controller
    {
        private readonly ICalendarioService _calendarioService;

        public CalendariosController(ICalendarioService calendarioService)
        {
            _calendarioService = calendarioService;
        }

        public ActionResult Index()
        {
            var model = _calendarioService.ObtenerEventos();
            return View(model);
        }

        [HttpGet]
        public JsonResult ObtenerEventos()
        {
            var eventos = _calendarioService.ObtenerEventos();

            return Json(eventos.Select(e => new
            {
                id = e.Id,
                title = e.Titulo,
                start = e.FechaInicio,
                end = e.FechaFinal,
                description = e.Descripcion
            }));
        }

        [HttpPost]
        public JsonResult CrearEvento([FromBody] Calendario evento)
        {
            try
            {
                if (evento == null)
                    return Json(new { success = false, message = "Datos inválidos." });

                _calendarioService.AgregarEvento(evento);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult EditarEvento([FromBody] Calendario evento)
        {
            try
            {
                if (evento == null)
                    return Json(new { success = false, message = "Datos inválidos." });

                _calendarioService.EditarEvento(evento);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult EliminarEvento(int id)
        {
            try
            {
                _calendarioService.EliminarEvento(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
