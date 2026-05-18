using System.Data.Entity;
using System.Text.Json;
using Humanizer.Localisation.TimeToClockNotation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Timely.Models;
using static Timely.Models.Proyectos;
using System.Linq;

namespace Timely.Controllers
{
	// Controlador encargado de la gestión de proyectos
	public class ProyectosController : Controller
	{
		// Servicio que contiene la lógica de negocio para los proyectos
		private Service Service;

		// Constructor que inicializa el servicio
		public ProyectosController()
		{
			Service = new Service();
		}

		// GET: ProyectosController
		// Muestra todos los proyectos en el tablero principal
		public ActionResult Tablero()
		{
			var model = Service.mostrarProyecto(); // Llama al servicio para obtener la lista de proyectos
			return View(model); // Devuelve la vista con el modelo
		}

		// GET: ProyectosController/Details/5
		// Muestra detalles de un proyecto (no implementado aún)
		public ActionResult Details(int id)
		{
			return View();
		}

		// GET: ProyectosController/Create
		// Muestra el formulario para crear un nuevo proyecto
		public ActionResult Create()
		{
			return View();
		}

		// POST: ProyectosController/Create
		// Recibe los datos del formulario y crea un nuevo proyecto
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create(Proyectos proyecto)
		{
			try
			{
				if (ModelState.IsValid)
				{
					Service.agregarProyecto(proyecto); // Agrega el proyecto
					return RedirectToAction("Tablero"); // Redirige al tablero
				}
			}
			catch
			{
				// Muestra un mensaje si el proyecto ya existe
				ModelState.AddModelError("", "El proyecto que deseas registrar ya está agendado. Intenta con otro.");
			}
			return View(); // Si falla, vuelve al formulario
		}

		// GET: /vencidos
		// Devuelve una lista de proyectos que están vencidos (Estado = "Vencido")
		[HttpGet("vencidos")]
		public async Task<IActionResult> GetProyectosVencidos()
		{
			var proyectosVencidos = await Service.Proyectos.Where(p => p.Estado == "Vencido").ToListAsync();
			return Ok(proyectosVencidos); // Retorna los proyectos vencidos como JSON
		}

		// GET: ProyectosController/Edit/5
		// Muestra la vista de edición de un proyecto específico
		public ActionResult Edit(int id)
		{
			var idBuscado = Service.buscarProyecto(id); // Busca el proyecto por ID
			return View(idBuscado); // Carga la vista de edición con los datos
		}

		// POST: ProyectosController/Edit/5
		// Actualiza los datos del proyecto
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit(Proyectos idActualizado)
		{
			try
			{
				if (ModelState.IsValid)
				{
					Service.actualizarProyecto(idActualizado); // Actualiza el proyecto
					return RedirectToAction("Tablero");
				}
			}
			catch
			{
				// Podrías capturar el error para registrar
			}
			return View(); // Si falla, vuelve a la vista
		}

		// GET: ProyectosController/Delete/5
		// Elimina un proyecto por ID (desde un botón o enlace)
		public ActionResult Delete(int id)
		{
			try
			{
				var idEliminado = Service.buscarProyecto(id); // Busca el proyecto
				Service.eliminarProyecto(idEliminado); // Lo elimina
				return RedirectToAction("Tablero");
			}
			catch (Exception)
			{
				// Si falla la eliminación, igualmente vuelve al tablero
				return RedirectToAction("Tablero");
			}
		}

		// POST: ProyectosController/Delete/5
		// Método POST alternativo para eliminar un proyecto (no implementado correctamente)
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Delete(int id, IFormCollection collection)
		{
			try
			{
				return RedirectToAction(nameof(Index)); // Redirige (aunque no hay método Index definido)
			}
			catch
			{
				return View();
			}
		}

		// POST: Actualizar el estado de un proyecto dinámicamente (usado con AJAX)
		[HttpPost]
		public ActionResult ActualizarEstado([FromBody] JsonElement data)
		{
			// Extrae el ID y el valor booleano "completado" del JSON recibido
			int id = data.GetProperty("id").GetInt32();
			bool completado = data.GetProperty("completado").GetBoolean();

			var proyecto = Service.buscarProyecto(id); // Busca el proyecto
			if (proyecto == null)
				return NotFound(); // Si no existe, retorna 404

			// Asigna el nuevo estado según si fue marcado como "completado"
			if (completado)
			{
				proyecto.Estado = "Hecho";
			}
			else
			{
				DateTime hoy = DateTime.Now;
				if (hoy < proyecto.Fecha_de_inicio)
					proyecto.Estado = "Inactivo";
				else if (hoy > proyecto.Vence)
					proyecto.Estado = "Vencido";
				else
					proyecto.Estado = "En proceso";
			}

			Service.actualizarProyecto(proyecto); // Guarda el cambio

			// Retorna el nuevo estado y una clase CSS para actualizar en la interfaz
			return Json(new
			{
				nuevoEstado = proyecto.Estado,
				nuevaClase = GetEstadoClase(proyecto.Fecha_de_inicio, proyecto.Vence, proyecto.Estado)
			});
		}

		// Método auxiliar para determinar la clase CSS según el estado y fechas del proyecto
		string GetEstadoClase(DateTime fechaInicio, DateTime fechaVencimiento, string estado)
		{
			DateTime hoy = DateTime.Now;

			if (estado == "Hecho")
				return "estado-hecho";

			if (hoy < fechaInicio)
				return "estado-inactivo";

			if (hoy >= fechaInicio && hoy <= fechaVencimiento)
				return "estado-proceso";

			if (hoy > fechaVencimiento)
				return "estado-vencido";

			return "estado-normal";
		}
	}
}

