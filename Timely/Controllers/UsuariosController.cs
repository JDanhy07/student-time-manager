using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using System.Security.Claims;
using Timely.Models;
using System.Globalization;
using Timely.Services;
using Timely.Services.Interfaces;

namespace Timely.Controllers
{
    public class UsuariosController : Controller
    {
        private  IUsuarioService _usuarioService;
        private  ILogger<UsuariosController> _logger;
        private readonly ICalendarioService _calendarioService;
        private static bool TryFecha(string texto, out DateTime fecha) => DateTime.TryParse(texto, CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha);

        public UsuariosController(IUsuarioService usuarioService, ILogger<UsuariosController> logger, ICalendarioService calendarioService)
        {
            _usuarioService = usuarioService;
            _calendarioService = calendarioService;
            _logger = logger;
        }


        public ActionResult Login()
        {
            return View();
        }

        [Authorize]
        public ActionResult Index()
        {
            var es = new CultureInfo("es-CR");
            var ahora = DateTime.Now;
            var hoy = ahora.Date;

            var usuario = _usuarioService.ObtenerPerfil(User);
            int usuarioId = usuario?.Id ?? 0;

            // FechaInicio / FechaFinal son string en el modelo, así que se parsean aquí
            var eventos = new List<EventoResumen>();
            foreach (var e in _calendarioService.ObtenerEventosPorUsuario(usuarioId))
            {
                if (!TryFecha(e.FechaInicio, out var inicio)) continue;

                eventos.Add(new EventoResumen
                {
                    Id = e.Id,
                    Titulo = e.Titulo,
                    Descripcion = e.Descripcion,
                    Inicio = inicio,
                    Fin = TryFecha(e.FechaFinal, out var fin) ? (DateTime?)fin : null
                });
            }

            var fechaTexto = ahora.ToString("dddd, d 'de' MMMM 'de' yyyy", es);
            fechaTexto = char.ToUpper(fechaTexto[0]) + fechaTexto.Substring(1);

            var vm = new HogarDashboardViewModel
            {
                NombreUsuario = User.Identity?.Name ?? "",   // es el nombre de usuario (claim Name del Login)
                Saludo = ahora.Hour < 12 ? "Buenos días" : ahora.Hour < 19 ? "Buenas tardes" : "Buenas noches",
                FechaHoy = fechaTexto,
                EventosHoy = eventos.Count(x => x.Inicio.Date == hoy),
                EventosSemana = eventos.Count(x => x.Inicio.Date >= hoy && x.Inicio.Date <= hoy.AddDays(7)),
                ProximosEventos = eventos
                    .Where(x => (x.Fin ?? x.Inicio) >= ahora)   // incluye los que están en curso
                    .OrderBy(x => x.Inicio)
                    .Take(5)
                    .ToList()
            };

            return View(vm);
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
            user.Rol = "Estudiante"; // Asigna el rol "Estudiante" al usuario creado
            ModelState.Remove("Rol"); // Elimina la validación del rol, ya que lo estamos asignando manualmente

            try
            {
                if (ModelState.IsValid)
                {
                    _usuarioService.AgregarUsuario(user); // Guarda el usuario en la base de datos 
                    return RedirectToAction("Login");
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
                    var rol = usuarioEncontrado.Rol;
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

        [AllowAnonymous]
        public ActionResult AccesoDenegado()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public ActionResult Edit(int id)
        {
            var usuario = _usuarioService.BuscarPorId(id);
            usuario.Contrasena = string.Empty;
            usuario.Confirmacion = string.Empty;
            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Usuarios model) //Se nombra "model" para evitar conflictos
        {
            if (string.IsNullOrEmpty(model.Contrasena))
            {
                ModelState.Remove("Contrasena");
                ModelState.Remove("Confirmacion");
                model.Contrasena = null;
            } else if (model.Contrasena != model.Confirmacion)
            {
                ModelState.AddModelError("Confirmacion", "Las contraseñas no coinciden");
            }

                try
                {
                    if (!ModelState.IsValid)
                        return View(model);

                    _usuarioService.ActualizarUsuario(model);
                    return RedirectToAction(nameof(Lista));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    return View(model);
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

        [Authorize(Roles = "Administrador")]
        public ActionResult CreateLista()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateLista(Usuarios user)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _usuarioService.AgregarUsuario(user);
                    return RedirectToAction(nameof(Lista));
                }
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario desde la administracion");
                ModelState.AddModelError("", "Error: " + ex.Message);
            }

            return View(user);
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
