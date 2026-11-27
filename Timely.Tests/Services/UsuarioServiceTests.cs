using System.Security.Claims;
using Timely.Models;
using Timely.Services;
using Timely.Tests.Helpers;

namespace Timely.Tests.Services
{
    public class UsuarioServiceTests : IDisposable
    {
        private readonly TestDb _db = new TestDb();
        private readonly UsuarioService _servicio;

        public UsuarioServiceTests()
        {
            _servicio = new UsuarioService(_db.Context);
        }

        public void Dispose() => _db.Dispose();

        private static Usuarios NuevoUsuario(string nombre, string correo) => new Usuarios
        {
            Usuario = nombre,
            Correo = correo,
            Contrasena = "Secreta123",
            Rol = "Estudiante",
            Fecha = "2000-01-01"
        };

        [Fact]
        public void AgregarUsuario_GuardaLaContrasenaHasheada()
        {
            _servicio.AgregarUsuario(NuevoUsuario("ana", "ana@test.com"));

            var guardado = _db.NuevoContexto().Usuarios.Single(u => u.Usuario == "ana");
            Assert.NotEqual("Secreta123", guardado.Contrasena);
        }

        [Fact]
        public void AgregarUsuario_NombreDuplicado_LanzaInvalidOperationException()
        {
            _servicio.AgregarUsuario(NuevoUsuario("ana", "ana@test.com"));

            var ex = Assert.Throws<InvalidOperationException>(() =>
                _servicio.AgregarUsuario(NuevoUsuario("ana", "otro@test.com")));

            Assert.Equal("El nombre del usuario ya esta en uso. Por favor, elija otro nombre de usuario.", ex.Message);
        }

        [Fact]
        public void AgregarUsuario_CorreoDuplicado_LanzaInvalidOperationException()
        {
            _servicio.AgregarUsuario(NuevoUsuario("ana", "ana@test.com"));

            var ex = Assert.Throws<InvalidOperationException>(() =>
                _servicio.AgregarUsuario(NuevoUsuario("beto", "ana@test.com")));

            Assert.Equal("El correo electrónico ya esta en uso. Por favor, elija otro correo electrónico.", ex.Message);
        }

        [Fact]
        public void ActualizarUsuario_ConElNombreDeOtroUsuario_LanzaInvalidOperationException()
        {
            _servicio.AgregarUsuario(NuevoUsuario("ana", "ana@test.com"));
            _servicio.AgregarUsuario(NuevoUsuario("beto", "beto@test.com"));
            var beto = _db.Context.Usuarios.Single(u => u.Usuario == "beto");

            var cambios = NuevoUsuario("ana", "beto@test.com");
            cambios.Id = beto.Id;
            cambios.Contrasena = null;

            Assert.Throws<InvalidOperationException>(() => _servicio.ActualizarUsuario(cambios));
        }

        [Fact]
        public void ActualizarUsuario_ConservandoSuPropioNombre_NoLanzaError()
        {
            _servicio.AgregarUsuario(NuevoUsuario("ana", "ana@test.com"));
            var ana = _db.Context.Usuarios.Single(u => u.Usuario == "ana");

            var cambios = NuevoUsuario("ana", "ana@test.com");
            cambios.Id = ana.Id;
            cambios.Contrasena = null;
            cambios.Fecha = "1999-12-31";

            _servicio.ActualizarUsuario(cambios);

            var guardado = _db.NuevoContexto().Usuarios.Single(u => u.Id == ana.Id);
            Assert.Equal("1999-12-31", guardado.Fecha);
        }

        [Fact]
        public void ActualizarPerfil_CambiaSoloUsuarioCorreoYFecha_NoTocaRolNiContrasena()
        {
            _servicio.AgregarUsuario(NuevoUsuario("ana", "ana@test.com"));
            var ana = _db.Context.Usuarios.Single(u => u.Usuario == "ana");
            var hashOriginal = ana.Contrasena;

            _servicio.ActualizarPerfil(ana.Id, "  ana2  ", " ana2@test.com ", "1999-12-31");

            var guardado = _db.NuevoContexto().Usuarios.Single(u => u.Id == ana.Id);
            Assert.Equal("ana2", guardado.Usuario);
            Assert.Equal("ana2@test.com", guardado.Correo);
            Assert.Equal("1999-12-31", guardado.Fecha);
            Assert.Equal("Estudiante", guardado.Rol);
            Assert.Equal(hashOriginal, guardado.Contrasena);
        }

        [Fact]
        public void ActualizarPerfil_ConElCorreoDeOtroUsuario_LanzaInvalidOperationException()
        {
            _servicio.AgregarUsuario(NuevoUsuario("ana", "ana@test.com"));
            _servicio.AgregarUsuario(NuevoUsuario("beto", "beto@test.com"));
            var beto = _db.Context.Usuarios.Single(u => u.Usuario == "beto");

            Assert.Throws<InvalidOperationException>(() =>
                _servicio.ActualizarPerfil(beto.Id, "beto", "ana@test.com", "2000-01-01"));
        }

        [Fact]
        public void ActualizarPerfil_ConservandoSusPropiosDatos_NoLanzaError()
        {
            _servicio.AgregarUsuario(NuevoUsuario("ana", "ana@test.com"));
            var ana = _db.Context.Usuarios.Single(u => u.Usuario == "ana");

            _servicio.ActualizarPerfil(ana.Id, "ana", "ana@test.com", "2001-02-03");

            Assert.Equal("2001-02-03", _db.NuevoContexto().Usuarios.Single(u => u.Id == ana.Id).Fecha);
        }

        [Fact]
        public void CambiarContrasena_ConLaActualCorrecta_GuardaLaNueva()
        {
            _servicio.AgregarUsuario(NuevoUsuario("ana", "ana@test.com"));
            var ana = _db.Context.Usuarios.Single(u => u.Usuario == "ana");

            var cambiada = _servicio.CambiarContrasena(ana.Id, "Secreta123", "Nueva456");

            Assert.True(cambiada);
            Assert.Null(_servicio.Login("ana", "Secreta123"));
            Assert.NotNull(_servicio.Login("ana", "Nueva456"));
        }

        [Fact]
        public void CambiarContrasena_ConLaActualIncorrecta_NoCambiaNada()
        {
            _servicio.AgregarUsuario(NuevoUsuario("ana", "ana@test.com"));
            var ana = _db.Context.Usuarios.Single(u => u.Usuario == "ana");

            var cambiada = _servicio.CambiarContrasena(ana.Id, "incorrecta", "Nueva456");

            Assert.False(cambiada);
            Assert.NotNull(_servicio.Login("ana", "Secreta123"));
        }

        [Fact]
        public void Login_ConCredencialesCorrectas_DevuelveElUsuario()
        {
            _servicio.AgregarUsuario(NuevoUsuario("ana", "ana@test.com"));

            var resultado = _servicio.Login("ana", "Secreta123");

            Assert.NotNull(resultado);
            Assert.Equal("ana", resultado.Usuario);
        }

        [Fact]
        public void Login_ConContrasenaIncorrecta_DevuelveNull()
        {
            _servicio.AgregarUsuario(NuevoUsuario("ana", "ana@test.com"));

            Assert.Null(_servicio.Login("ana", "incorrecta"));
        }

        [Fact]
        public void Login_ConUsuarioInexistente_DevuelveNull()
        {
            Assert.Null(_servicio.Login("nadie", "Secreta123"));
        }

        [Fact]
        public void ObtenerPerfil_ResuelveElUsuarioPorElClaimDeId()
        {
            var ana = _db.CrearUsuario("ana");
            _db.CrearUsuario("beto");

            // El nombre del claim no coincide a propósito: debe mandar el Id
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, ana.Id.ToString()),
                new Claim(ClaimTypes.Name, "beto")
            }, "PruebasAuth"));

            var perfil = _servicio.ObtenerPerfil(principal);

            Assert.Equal(ana.Id, perfil.Id);
        }

        [Fact]
        public void ObtenerPerfil_SinClaimDeId_ResuelvePorNombre()
        {
            var ana = _db.CrearUsuario("ana");

            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "ana")
            }, "PruebasAuth"));

            var perfil = _servicio.ObtenerPerfil(principal);

            Assert.Equal(ana.Id, perfil.Id);
        }

        // ---------- Buscar (filtro, orden y paginación en la base de datos) ----------

        [Fact]
        public void Buscar_PorTexto_EncuentraPorUsuarioOCorreo()
        {
            _db.CrearUsuario("ana");
            _db.CrearUsuario("beto");
            _db.CrearUsuario("carla");

            var porNombre = _servicio.Buscar("BET", null, "nombre", 1, 10);
            var porCorreo = _servicio.Buscar("carla@test", null, "nombre", 1, 10);

            Assert.Equal("beto", Assert.Single(porNombre.Items).Usuario);
            Assert.Equal("carla", Assert.Single(porCorreo.Items).Usuario);
        }

        [Fact]
        public void Buscar_LosComodinesDelTextoSeBuscanTalCual()
        {
            _db.CrearUsuario("ana_01");
            _db.CrearUsuario("anaX01");

            var resultado = _servicio.Buscar("ana_", null, "nombre", 1, 10);

            Assert.Equal("ana_01", Assert.Single(resultado.Items).Usuario);
        }

        [Fact]
        public void Buscar_FiltraPorRol()
        {
            _db.CrearUsuario("ana", "Administrador");
            _db.CrearUsuario("beto");
            _db.CrearUsuario("carla");

            var admins = _servicio.Buscar(null, "Administrador", "nombre", 1, 10);
            var estudiantes = _servicio.Buscar(null, "Estudiante", "nombre", 1, 10);

            Assert.Equal(1, admins.Total);
            Assert.Equal(2, estudiantes.Total);
        }

        [Fact]
        public void Buscar_PaginaYAjustaLaPaginaFueraDeRango()
        {
            for (int i = 1; i <= 25; i++) _db.CrearUsuario($"user{i:00}");

            var primera = _servicio.Buscar(null, null, "nombre", 1, 10);
            var ultima = _servicio.Buscar(null, null, "nombre", 99, 10);

            Assert.Equal(25, primera.Total);
            Assert.Equal(3, primera.TotalPaginas);
            Assert.Equal(10, primera.Items.Count);
            Assert.Equal(3, ultima.Pagina);
            Assert.Equal(5, ultima.Items.Count);
        }

        [Fact]
        public void Buscar_OrdenaPorMasRecientes()
        {
            var primero = _db.CrearUsuario("ana");
            var segundo = _db.CrearUsuario("beto");

            var resultado = _servicio.Buscar(null, null, "recientes", 1, 10);

            Assert.Equal(segundo.Id, resultado.Items[0].Id);
            Assert.Equal(primero.Id, resultado.Items[1].Id);
        }

        [Fact]
        public void ContarPorRol_AgrupaPorRol()
        {
            _db.CrearUsuario("ana", "Administrador");
            _db.CrearUsuario("beto");
            _db.CrearUsuario("carla");

            var conteos = _servicio.ContarPorRol();

            Assert.Equal(1, conteos["Administrador"]);
            Assert.Equal(2, conteos["Estudiante"]);
        }
    }
}