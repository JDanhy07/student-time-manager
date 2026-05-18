using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Timely.Models;

namespace Timely.Controllers
{
	// Controlador encargado de manejar las acciones relacionadas con el calendario
	public class CalendariosController : Controller
	{
		// Instancia del servicio que contiene la lógica del negocio para el calendario
		private Service Service;

		// Constructor del controlador, inicializa el servicio
		public CalendariosController()
		{
			Service = new Service();
		}

		// Acción que muestra la vista principal del calendario
		// GET: CalendariosController
		public ActionResult Index()
		{
			// Obtiene la lista de eventos desde el servicio
			var model = Service.mostrarCalendario();
			return View(model); // Devuelve la vista con el modelo de eventos
		}

		// Acción que devuelve los eventos en formato JSON para el componente FullCalendar
		// GET: Obtener eventos para FullCalendar
		[HttpGet]
		public JsonResult ObtenerEventos()
		{
			var eventos = Service.mostrarCalendario(); // Obtiene eventos

			// Convierte los eventos a un formato que FullCalendar puede usar y los devuelve en JSON
			return Json(eventos.Select(e => new
			{
				id = e.Id,
				title = e.Titulo,
				start = e.FechaInicio,
				end = e.FechaFinal,
				description = e.Descripcion
			}));
		}

		// Acción GET para crear evento (no hace nada útil aquí, solo devuelve un JSON de éxito)
		// GET: CalendariosController/Create
		public JsonResult CrearEvento()
		{
			return Json(new { success = true });
		}

		// Acción POST para crear un nuevo evento
		[HttpPost]
		public JsonResult CrearEvento([FromBody] Calendario eventos)
		{
			try
			{
				// Verifica que los datos del evento no sean nulos
				if (eventos != null)
				{
					Service.agregarEvento(eventos); // Llama al servicio para agregar el evento
					return Json(new { success = true }); // Retorna éxito
				}
				return Json(new { success = false, message = "Datos inválidos." }); // Retorna error si los datos son nulos
			}
			catch (Exception ex)
			{
				// Retorna error si ocurre una excepción
				return Json(new { success = false, message = ex.Message });
			}
		}

		// Acción GET para editar un evento (similar al create, pero no hace nada útil aquí)
		// GET: CalendariosController/Edit/5
		public JsonResult EditarEvento(int id)
		{
			return Json(new { success = true });
		}

		// Acción POST para editar un evento existente
		[HttpPost]
		public JsonResult EditarEvento([FromBody] Calendario evento)
		{
			try
			{
				// Verifica que el evento no sea nulo
				if (evento != null)
				{
					Service.editarEvento(evento); // Llama al servicio para actualizar el evento
					return Json(new { success = true });
				}
				return Json(new { success = false, message = "Datos inválidos." });
			}
			catch (Exception ex)
			{
				// Retorna mensaje de error si ocurre una excepción
				return Json(new { success = false, message = ex.Message });
			}
		}

		// Acción GET para eliminar (muestra vista, no se usa en lógica JSON de FullCalendar)
		// GET: CalendariosController/Delete/5
		public ActionResult Delete(int id)
		{
			return View();
		}

		// Acción POST para eliminar un evento
		[HttpPost]
		public JsonResult EliminarEvento(int id)
		{
			try
			{
				Service.eliminarEvento(id); // Llama al servicio para eliminar el evento por su id
				return Json(new { success = true });
			}
			catch (Exception ex)
			{
				// Retorna error si ocurre una excepción
				return Json(new { success = false, message = ex.Message });
			}
		}
	}
}
