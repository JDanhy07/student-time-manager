using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Timely.Models;

namespace Timely.Controllers
{
	// Controlador principal que maneja las vistas generales del sitio (Home, Privacy, Error)
	public class HomeController : Controller
	{
		// Logger para registrar información, errores, etc.
		private readonly ILogger<HomeController> _logger;

		// Constructor que inyecta el logger al controlador
		public HomeController(ILogger<HomeController> logger)
		{
			_logger = logger;
		}

		// Acción que retorna la vista principal (Index)
		public IActionResult Index()
		{
			return View(); // Devuelve la vista "Index.cshtml"
		}

		// Acción que retorna la vista de privacidad
		public IActionResult Privacy()
		{
			return View(); // Devuelve la vista "Privacy.cshtml"
		}

		// Acción que maneja los errores y muestra información sobre ellos
		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			// Crea un modelo de error con el ID de la solicitud actual
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
