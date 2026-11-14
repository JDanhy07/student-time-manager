using Timely.Services;
using Timely.Tests.Helpers;

namespace Timely.Tests.Services
{
    public class ProyectoServiceTests : IDisposable
    {
        private readonly TestDb _db = new TestDb();
        private readonly ProyectoService _servicio;

        public ProyectoServiceTests()
        {
            _servicio = new ProyectoService(_db.Context);
        }

        public void Dispose() => _db.Dispose();

        [Fact]
        public void ObtenerPorUsuario_SoloDevuelveLosProyectosDelUsuario()
        {
            var ana = _db.CrearUsuario("ana");
            var beto = _db.CrearUsuario("beto");
            _db.CrearProyecto(ana);
            _db.CrearProyecto(ana);
            _db.CrearProyecto(beto);

            var proyectos = _servicio.ObtenerPorUsuario(ana.Id);

            Assert.Equal(2, proyectos.Count);
            Assert.All(proyectos, p => Assert.Equal(ana.Id, p.UsuarioId));
        }

        [Fact]
        public async Task ObtenerVencidosAsync_SoloDevuelveLosVencidosDelUsuario()
        {
            var ana = _db.CrearUsuario("ana");
            var beto = _db.CrearUsuario("beto");
            _db.CrearProyecto(ana, "Vencido");
            _db.CrearProyecto(ana, "En proceso");
            _db.CrearProyecto(beto, "Vencido");

            var vencidos = await _servicio.ObtenerVencidosAsync(ana.Id);

            var proyecto = Assert.Single(vencidos);
            Assert.Equal(ana.Id, proyecto.UsuarioId);
            Assert.Equal("Vencido", proyecto.Estado);
        }

        [Fact]
        public async Task ObtenerVencidosAsync_IncluyeProyectosVencidosAunqueElEstadoGuardadoSigaEnProceso()
        {
            var ana = _db.CrearUsuario("ana");
            var proyecto = _db.CrearProyecto(ana, "En proceso");

            // La fecha pasa sin que nadie edite el proyecto: en la BD el Estado sigue en "En proceso"
            proyecto.Vence = DateTime.Now.AddMinutes(-5);
            _db.Context.SaveChanges();

            var vencidos = await _servicio.ObtenerVencidosAsync(ana.Id);

            var resultado = Assert.Single(vencidos);
            Assert.Equal(proyecto.Id, resultado.Id);
        }

        [Fact]
        public async Task ObtenerVencidosAsync_NoIncluyeProyectosCompletadosAunqueYaHayanVencido()
        {
            var ana = _db.CrearUsuario("ana");
            var proyecto = _db.CrearProyecto(ana, "En proceso");

            proyecto.Vence = DateTime.Now.AddMinutes(-5);
            proyecto.Completado = true;
            _db.Context.SaveChanges();

            var vencidos = await _servicio.ObtenerVencidosAsync(ana.Id);

            Assert.Empty(vencidos);
        }
    }
}
