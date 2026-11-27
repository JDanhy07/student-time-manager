using Timely.Models;
using Timely.Services;
using Timely.Tests.Helpers;

namespace Timely.Tests.Services
{
    public class NotaServiceTests : IDisposable
    {
        private readonly TestDb _db = new TestDb();
        private readonly NotaService _servicio;

        public NotaServiceTests()
        {
            _servicio = new NotaService(_db.Context);
        }

        public void Dispose() => _db.Dispose();

        [Fact]
        public void ObtenerPorUsuario_SoloDevuelveLasNotasDelUsuario()
        {
            var ana = _db.CrearUsuario("ana");
            var beto = _db.CrearUsuario("beto");
            _db.CrearNota(ana);
            _db.CrearNota(beto);
            _db.CrearNota(beto);

            var notas = _servicio.ObtenerPorUsuario(beto.Id);

            Assert.Equal(2, notas.Count);
            Assert.All(notas, n => Assert.Equal(beto.Id, n.UsuarioId));
        }

        [Fact]
        public void ActualizarNota_ActualizaElTituloElContenidoYLaCarpeta()
        {
            var ana = _db.CrearUsuario("ana");
            var nota = _db.CrearNota(ana, "Título viejo");

            _servicio.ActualizarNota(new Nota
            {
                Id = nota.Id,
                Titulo = "Título nuevo",
                Contenido = "Contenido nuevo",
                Carpeta = "Estudio",
                UsuarioId = ana.Id
            });

            var guardada = _db.NuevoContexto().Notas.Single(n => n.Id == nota.Id);
            Assert.Equal("Título nuevo", guardada.Titulo);
            Assert.Equal("Contenido nuevo", guardada.Contenido);
            Assert.Equal("Estudio", guardada.Carpeta);
        }
    }
}
