using Timely.Data;
using Timely.Services;
using Timely.Tests.Helpers;

namespace Timely.Tests.Data
{
    public class DemoDataSeederTests : IDisposable
    {
        private readonly TestDb _db = new TestDb();

        public void Dispose() => _db.Dispose();

        private int IdDelEstudiante() =>
            _db.NuevoContexto().Usuarios.Single(u => u.Usuario == DemoDataSeeder.UsuarioEstudiante).Id;

        [Fact]
        public void Sembrar_EnBaseVacia_CreaElAdministradorYElEstudiante()
        {
            DemoDataSeeder.Sembrar(_db.Context);

            var usuarios = _db.NuevoContexto().Usuarios.ToList();
            Assert.Equal(2, usuarios.Count);
            Assert.Contains(usuarios, u => u.Usuario == DemoDataSeeder.UsuarioAdmin && u.Rol == "Administrador");
            Assert.Contains(usuarios, u => u.Usuario == DemoDataSeeder.UsuarioEstudiante && u.Rol == "Estudiante");
        }

        [Fact]
        public void Sembrar_LosUsuariosDeDemostracionPuedenIniciarSesion()
        {
            DemoDataSeeder.Sembrar(_db.Context);
            var servicio = new UsuarioService(_db.Context);

            Assert.NotNull(servicio.Login(DemoDataSeeder.UsuarioAdmin, DemoDataSeeder.ContrasenaAdmin));
            Assert.NotNull(servicio.Login(DemoDataSeeder.UsuarioEstudiante, DemoDataSeeder.ContrasenaEstudiante));
        }

        [Fact]
        public async Task Sembrar_CreaProyectosEnDistintosEstados()
        {
            DemoDataSeeder.Sembrar(_db.Context);
            int estudianteId = IdDelEstudiante();
            var servicio = new ProyectoService(_db.Context);

            var proyectos = servicio.ObtenerPorUsuario(estudianteId);
            var vencidos = await servicio.ObtenerVencidosAsync(estudianteId);

            Assert.Equal(4, proyectos.Count);
            Assert.Contains(proyectos, p => p.Completado);                      // Hecho
            Assert.Contains(proyectos, p => p.Fecha_de_inicio > DateTime.Now);  // Inactivo
            Assert.Single(vencidos);                                            // Vencido
        }

        [Fact]
        public void Sembrar_CreaNotasYEventosSoloParaElEstudiante()
        {
            DemoDataSeeder.Sembrar(_db.Context);
            int estudianteId = IdDelEstudiante();

            Assert.Equal(3, new NotaService(_db.Context).ObtenerPorUsuario(estudianteId).Count);
            Assert.Equal(4, new CalendarioService(_db.Context).ObtenerEventosPorUsuario(estudianteId).Count);
            Assert.Equal(3, _db.NuevoContexto().Notas.Count());
            Assert.Equal(4, _db.NuevoContexto().Calendario.Count());
        }

        [Fact]
        public void Sembrar_EjecutadoDosVeces_NoDuplicaLosDatos()
        {
            DemoDataSeeder.Sembrar(_db.Context);
            DemoDataSeeder.Sembrar(_db.Context);

            var contexto = _db.NuevoContexto();
            Assert.Equal(2, contexto.Usuarios.Count());
            Assert.Equal(4, contexto.Proyectos.Count());
        }

        [Fact]
        public void Sembrar_ConUnaBaseDeDatosYaUsada_NoAgregaNada()
        {
            _db.CrearUsuario("existente");

            DemoDataSeeder.Sembrar(_db.Context);

            var usuario = Assert.Single(_db.NuevoContexto().Usuarios.ToList());
            Assert.Equal("existente", usuario.Usuario);
        }
    }
}
