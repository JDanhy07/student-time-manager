using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Timely.Models;
using Timely.Services;
using Timely.Services.Interfaces;

namespace Timely.Controllers
{
    public class UsuariosController : Controller
    {
        private  IUsuarioService _usuarioService;
        private  ILogger<UsuariosController> _logger;

        public UsuariosController(IUsuarioService usuarioService, ILogger<UsuariosController> logger)
        {
            _usuarioService = usuarioService;
            _logger = logger;
        }


        public ActionResult Login()
        {
            return View();
        }

        public ActionResult Index()
        {
            var model = _usuarioService.ObtenerTodos();
            return View(model);
        }

        [Authorize(Roles = "Administrador")]
        public ActionResult Lista()
        {
            var model = _usuarioService.ObtenerTodos();
            return View(model);
        }


        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Usuarios user) //IMPORTANTE: ponerlo de nombre diferente al modelo para evitar conflictos de binding
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _usuarioService.AgregarUsuario(user); // Guarda el usuario en la base de datos 
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario");
                ModelState.AddModelError("", "Error: " + ex.Message);
                // También puedes ver la inner exception:
                if (ex.InnerException != null)
                    ModelState.AddModelError("", "Detalle: " + ex.InnerException.Message);
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(string usuario, string contrasena)
        {
            try
            {
                var usuarioEncontrado = _usuarioService.Login(usuario, contrasena);

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

                    return RedirectToAction("Index"); // Redirige según el flujo deseado
                }
                else
                {
                    Console.WriteLine("❌ Usuario o contraseña incorrectos.");
                    ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
                    return View("Login"); // Redirige a vista de error
                }
            }
            catch
            {
                // En caso de error inesperado
            }

            return View(); // Vista por defecto si algo sale mal
        }


        [Authorize(Roles = "Administrador")]
        public ActionResult Edit(int id)
        {
            var usuario = _usuarioService.BuscarPorId(id);
            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Usuarios usuario)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(usuario);

                _usuarioService.ActualizarUsuario(usuario);
                return RedirectToAction(nameof(Lista));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(usuario);
            }
        }

        [Authorize(Roles = "Administrador")]
        public ActionResult Delete(int id)
        {
            var usuario = _usuarioService.BuscarPorId(id);
            return View(usuario);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmado(int id)
        {
            try
            {
                var usuario = _usuarioService.BuscarPorId(id);
                _usuarioService.EliminarUsuario(usuario);
                return RedirectToAction(nameof(Lista));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario con ID {Id}.", id);
                return RedirectToAction(nameof(Lista));
            }
        }

        [Authorize]
        public ActionResult MiPerfil()
        {
            var usuario = _usuarioService.ObtenerPerfil(User);
            if (usuario == null)
                return NotFound();

            return View(usuario);
        }
    }
}
