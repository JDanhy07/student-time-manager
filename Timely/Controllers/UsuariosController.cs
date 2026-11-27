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
        private IUsuarioService _usuarioService;
        private ILogger<UsuariosController> _logger;
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
        public ActionResult Lista(string? q, string? rol, string? orden, int pagina = 1)
        {
            // Solo se aceptan los roles y órdenes conocidos; cualquier otro valor se ignora
            rol = rol is "Administrador" or "Estudiante" ? rol : null;
            orden = orden is "recientes" or "antiguos" ? orden : "nombre";

            // El filtro, el orden y la paginación los hace la base de datos (ver UsuarioService.Buscar)
            var resultado = _usuarioService.Buscar(q, rol, orden, pagina, ListaUsuariosViewModel.TamanoPagina);
            var conteos = _usuarioService.ContarPorRol();

            var vm = new ListaUsuariosViewModel
            {
                Items = resultado.Items,
                Busqueda = q,
                Rol = rol,
                Orden = orden,
                Pagina = resultado.Pagina,
                TotalPaginas = resultado.TotalPaginas,
                TotalFiltrado = resultado.Total,
                TotalAdministradores = conteos.GetValueOrDefault("Administrador"),
                TotalEstudiantes = conteos.GetValueOrDefault("Estudiante"),
                Total = conteos.Values.Sum()
            };

            return View(vm);
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
            catch (InvalidOperationException ex)
            {
                //Nombre del usuario o correo duplicado
                ModelState.AddModelError("", ex.Message);
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
                    await IniciarSesionAsync(usuarioEncontrado);

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

        // Crea los claims (id, nombre, rol) y emite la cookie de sesión.
        // También se usa al cambiar el nombre de usuario, para que la barra superior muestre el nombre nuevo.
        private async Task IniciarSesionAsync(Usuarios usuario)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Usuario),
                new Claim(ClaimTypes.Role, usuario.Rol)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                IsPersistent = false, // Cambiar a true si quieres que la sesión persista después de cerrar el navegador
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            HttpContext.Session.SetString("Usuario", usuario.Usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Logout()
        {
            //Elimina la cookie de autenticación y cierra la sesión del usuario
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear(); // Limpia la sesión

            return RedirectToAction("Login");
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
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Usuarios model) //Se nombra "model" para evitar conflictos
        {
            if (string.IsNullOrEmpty(model.Contrasena))
            {
                ModelState.Remove("Contrasena");
                ModelState.Remove("Confirmacion");
                model.Contrasena = null;
            }
            else if (model.Contrasena != model.Confirmacion)
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
        [Authorize(Roles = "Administrador")]
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
        [Authorize(Roles = "Administrador")]
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

            catch (InvalidOperationException ex)
            {
                //Nombre del usuario o correo duplicado
                ModelState.AddModelError("", ex.Message);
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

            return View(ArmarPerfil(usuario));
        }

        // El <input type="date"> solo entiende yyyy-MM-dd; los valores guardados con otro formato se normalizan
        private static string NormalizarFecha(string texto) =>
            TryFecha(texto, out var fecha) ? fecha.ToString("yyyy-MM-dd") : texto;

        private static MiPerfilViewModel ArmarPerfil(Usuarios usuario) => new MiPerfilViewModel
        {
            Id = usuario.Id,
            Usuario = usuario.Usuario,
            Rol = usuario.Rol,
            Correo = usuario.Correo,
            Fecha = usuario.Fecha,
            Edicion = new EditarPerfilViewModel
            {
                Usuario = usuario.Usuario,
                Correo = usuario.Correo,
                Fecha = NormalizarFecha(usuario.Fecha)
            }
        };

        // La persona solo puede editar SU perfil: el id sale de la sesión, nunca del formulario
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ActualizarPerfil([Bind(Prefix = "Edicion")] EditarPerfilViewModel edicion)
        {
            var usuario = _usuarioService.ObtenerPerfil(User);
            if (usuario == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _usuarioService.ActualizarPerfil(usuario.Id, edicion.Usuario, edicion.Correo, edicion.Fecha);

                    // Se vuelve a emitir la cookie para que el nombre nuevo aparezca en la barra superior
                    await IniciarSesionAsync(_usuarioService.BuscarPorId(usuario.Id));

                    TempData["PerfilMensaje"] = "Tus datos se actualizaron correctamente.";
                    return RedirectToAction(nameof(MiPerfil));
                }
                catch (InvalidOperationException ex)
                {
                    // Usuario o correo ya en uso
                    ModelState.AddModelError("", ex.Message);
                }
            }

            var vm = ArmarPerfil(usuario);
            vm.Edicion = edicion;
            vm.EditandoDatos = true;
            return View("MiPerfil", vm);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult CambiarContrasena([Bind(Prefix = "Contrasena")] CambiarContrasenaViewModel contrasena)
        {
            var usuario = _usuarioService.ObtenerPerfil(User);
            if (usuario == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                if (_usuarioService.CambiarContrasena(usuario.Id, contrasena.ContrasenaActual, contrasena.ContrasenaNueva))
                {
                    TempData["PerfilMensaje"] = "Tu contraseña se cambió correctamente.";
                    return RedirectToAction(nameof(MiPerfil));
                }

                ModelState.AddModelError("Contrasena.ContrasenaActual", "La contraseña actual no es correcta.");
            }

            var vm = ArmarPerfil(usuario);
            vm.CambiandoContrasena = true;   // las contraseñas escritas no se devuelven a la vista
            return View("MiPerfil", vm);
        }
    }

}