using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Timely.Models;

namespace Timely.Controllers
{
	// Controlador encargado de manejar las acciones relacionadas con las notas
	public class NotasController : Controller
	{
		// Instancia del servicio que maneja la lógica de negocio de las notas
		private Service Service;

		// Constructor que inicializa el servicio
		public NotasController()
		{
			Service = new Service();
		}

		// Acción que muestra todas las notas del usuario
		// GET: NotasController
		public ActionResult MisNotas()
		{
			var model = Service.mostrarNota(); // Llama al servicio para obtener las notas
			return View(model); // Muestra la vista con el modelo de notas
		}

		// Acción GET que devuelve la vista para crear una nueva nota
		// GET: NotasController/Create
		public ActionResult Create()
		{
			return View();
		}

		// Acción POST que recibe los datos del formulario para crear una nueva nota
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create(Nota notas)
		{
			try
			{
				// Verifica que el modelo sea válido (que todos los campos requeridos estén bien)
				if (ModelState.IsValid)
				{
					Service.agregarNota(notas); // Llama al servicio para guardar la nueva nota
					return RedirectToAction("MisNotas"); // Redirige a la lista de notas
				}
			}
			catch
			{
				// Si ocurre un error (por ejemplo, una nota duplicada), se muestra un mensaje de error
				ModelState.AddModelError("", "La nota que deseas añadir ya existe. Favor, Intenta con otra.");
			}
			return View(); // Si falla, regresa a la misma vista con los errores
		}

		// Acción GET que devuelve la vista para editar una nota específica
		// GET: NotasController/Edit/5
		public ActionResult Edit(int id)
		{
			var idBuscados = Service.buscarIdnota(id); // Busca la nota por su ID
			return View(idBuscados); // Muestra la vista de edición con los datos actuales
		}

		// Acción POST que actualiza los datos de una nota existente
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit(int id, Nota idActualizadonota)
		{
			try
			{
				if (ModelState.IsValid)
				{
					Service.actualizarNotas(idActualizadonota); // Llama al servicio para actualizar la nota
					return RedirectToAction("MisNotas"); // Redirige a la lista de notas
				}
			}
			catch
			{
				// Aquí podrías capturar el error si lo deseas
			}
			return View(); // Si falla, vuelve a la vista de edición
		}

		// Acción GET que devuelve la vista para confirmar la eliminación de una nota
		// GET: NotasController/Delete/5
		public ActionResult Delete(int id)
		{
			return View(); // En este ejemplo no carga la nota a eliminar, pero podrías hacerlo
		}

		// Acción POST que elimina una nota específica
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Delete(int id, IFormCollection collection)
		{
			try
			{
				var idEliminados = Service.buscarIdnota(id); // Busca la nota a eliminar por ID
				Service.eliminarIdnotas(idEliminados); // Elimina la nota usando el servicio
				return RedirectToAction("MisNotas"); // Redirige a la lista de notas
			}
			catch (Exception)
			{
				return View("MisNotas"); // Si falla, vuelve a la vista de notas
			}
		}
	}
}
