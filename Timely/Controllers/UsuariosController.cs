using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Data.Entity;
using System.Runtime.InteropServices;
using System.Security.Claims;
using Timely.Models;

namespace Timely.Controllers
{
	public class UsuariosController : Controller
	{
		private Service Service;

		public UsuariosController()
		{
			Service = new Service();
		}

		// GET: UsuarioController/Login
		// Muestra la vista de login
		public ActionResult Login()
		{
			return View();
		}

		// GET: UsuariosController
		// Muestra una lista general de usuarios (público o para pruebas)
		public ActionResult Index()
		{
			var model = Service.mostrarUsuario();
			return View(model);
		}

		// GET: UsuariosController/Administrador
		// Solo los administradores pueden ver esta lista completa de usuarios
		[Authorize(Roles = "Administrador")]
		public ActionResult Lista()
		{
			var model = Service.mostrarUsuario();
			return View(model);
		}

		// GET: UsuariosController/Create
		// Muestra el formulario para registrar un nuevo usuario
		public ActionResult Create()
		{
			return View();
		}

		// POST: UsuariosController/Create
		// Recibe los datos del formulario para crear un usuario
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create(Usuarios usuari)
		{
			try
			{
				if (ModelState.IsValid)
				{
					Service.agregarUsuario(usuari); // Guarda el usuario
					return RedirectToAction("Index");
				}
			}
			catch
			{
				// Si ocurre un error, muestra el mensaje personalizado
				ModelState.AddModelError("", "El usuario que deseas registrar ya está ocupado. Intenta con otro.");
			}
			return View();
		}

		// POST: UsuarioController/Login
		// Procesa el inicio de sesión de un usuario
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Login(string usuario, int contrasena)
		{
			try
			{
				var usuarioEncontrado = Service.loginUsuarios(usuario, contrasena);

				if (usuarioEncontrado != null)
				{
					// Crea los claims para establecer la sesión y rol
					var rol = usuarioEncontrado.Perfil;
					var claims = new List<Claim>()
					{
						new Claim(ClaimTypes.Name, usuarioEncontrado.Usuario),
						new Claim(ClaimTypes.Role, rol)
					};

					var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
					var authProperties = new AuthenticationProperties
					{
						AllowRefresh = true,
						IsPersistent = true
					};

					// Inicia la sesión del usuario
					await HttpContext.SignInAsync(
						CookieAuthenticationDefaults.AuthenticationScheme,
						new ClaimsPrincipal(claimsIdentity),
						authProperties);

					HttpContext.Session.SetString("Usuario", usuarioEncontrado.Usuario);
					Console.WriteLine("✅ Sesión iniciada correctamente.");

					return RedirectToAction("Login"); // Redirige según el flujo deseado
				}
				else
				{
					Console.WriteLine("❌ Usuario o contraseña incorrectos.");
					ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
					return View("Index"); // Redirige a vista de error
				}
			}
			catch
			{
				// En caso de error inesperado
			}

			return View(); // Vista por defecto si algo sale mal
		}

		// GET: UsuariosController/Edit/5
		// Solo administradores pueden editar usuarios
		[Authorize(Roles = "Administrador")]
		public ActionResult Edit(int id)
		{
			var idBuscado = Service.buscarId(id);
			return View(idBuscado);
		}

		// POST: UsuariosController/Edit/5
		// Actualiza los datos de un usuario
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit(Usuarios idActualizado)
		{
			try
			{
				if (ModelState.IsValid)
				{
					Service.actualizarId(idActualizado);
					return RedirectToAction("Lista");
				}
			}
			catch
			{
				// Aquí puedes registrar errores si lo deseas
			}
			return View();
		}

		// GET: UsuariosController/Delete/5
		// Muestra la vista para confirmar la eliminación
		[Authorize(Roles = "Administrador")]
		public ActionResult Delete(int id)
		{
			return View();
		}

		// POST: UsuariosController/Delete/5
		// Elimina al usuario seleccionado
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Delete(int id, IFormCollection collection)
		{
			try
			{
				var idEliminado = Service.buscarId(id);
				Service.eliminarId(idEliminado);
				return RedirectToAction("Lista");
			}
			catch (Exception)
			{
				return View("Lista"); // Vuelve a la lista en caso de error
			}
		}

		// GET: UsuariosController/MiPerfil
		// Solo usuarios logueados pueden ver su perfil
		[Authorize]
		public ActionResult MiPerfil()
		{
			var usuario = Service.MiPerfil(User);
			if (usuario != null)
			{
				return View(usuario); // Devuelve el perfil del usuario actual
			}
			else
			{
				return NotFound(); // Si no lo encuentra
			}
		}

		// GET: UsuariosController/CreateLista
		// Permite crear usuarios desde la vista "Lista"
		public ActionResult CreateLista()
		{
			return View();
		}

		// POST: UsuariosController/CreateLista
		// Crea usuarios desde la vista Lista (similar al Create común)
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult CreateLista(Usuarios usuari)
		{
			try
			{
				if (ModelState.IsValid)
				{
					Service.agregarUsuario(usuari);
					return RedirectToAction("Lista");
				}
			}
			catch
			{
				ModelState.AddModelError("", "El usuario que deseas registrar ya está ocupado. Intenta con otro.");
			}
			return View();
		}
	}
}

