using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Timely.Controllers;
using Timely.Models;
using Timely.Services;
using Timely.Tests.Helpers;

namespace Timely.Tests.Controllers
{
    // Un usuario autenticado nunca debe poder ver ni modificar datos de otro.
    // Se usan los servicios reales sobre SQLite en memoria; solo la sesión es simulada.
    public class AislamientoDeDatosTests : IDisposable
    {
        private readonly TestDb _db = new TestDb();
        private readonly Usuarios _ana;
        private readonly Usuarios _beto;

        public AislamientoDeDatosTests()
        {
            _ana = _db.CrearUsuario("ana");
            _beto = _db.CrearUsuario("beto");
        }

        public void Dispose() => _db.Dispose();

        private ProyectosController ProyectosComo(Usuarios usuario) =>
            new ProyectosController(new ProyectoService(_db.Context), new UsuarioService(_db.Context))
                .ComoUsuario(usuario);

        private NotasController NotasComo(Usuarios usuario) =>
            new NotasController(new NotaService(_db.Context), new UsuarioService(_db.Context))
                .ComoUsuario(usuario);

        private CalendariosController CalendariosComo(Usuarios usuario) =>
            new CalendariosController(new CalendarioService(_db.Context), new UsuarioService(_db.Context))
                .ComoUsuario(usuario);

        // ---------- Proyectos ----------

        [Fact]
        public void ActualizarEstado_ProyectoAjeno_DevuelveUnauthorizedYNoModificaNada()
        {
            var proyectoDeAna = _db.CrearProyecto(_ana);
            using var datos = JsonDocument.Parse($"{{\"id\":{proyectoDeAna.Id},\"completado\":true}}");

            var resultado = ProyectosComo(_beto).ActualizarEstado(datos.RootElement);

            Assert.IsType<UnauthorizedResult>(resultado);
            var guardado = _db.NuevoContexto().Proyectos.Single(p => p.Id == proyectoDeAna.Id);
            Assert.Equal("En proceso", guardado.Estado);
            Assert.False(guardado.Completado);
        }

        [Fact]
        public void ActualizarEstado_ProyectoPropio_MarcaElProyectoComoHecho()
        {
            var proyectoDeAna = _db.CrearProyecto(_ana);
            using var datos = JsonDocument.Parse($"{{\"id\":{proyectoDeAna.Id},\"completado\":true}}");

            var resultado = ProyectosComo(_ana).ActualizarEstado(datos.RootElement);

            Assert.IsType<JsonResult>(resultado);
            var guardado = _db.NuevoContexto().Proyectos.Single(p => p.Id == proyectoDeAna.Id);
            Assert.Equal("Hecho", guardado.Estado);
            Assert.True(guardado.Completado);
        }

        [Fact]
        public void Edit_ProyectoAjeno_DevuelveUnauthorized()
        {
            var proyectoDeAna = _db.CrearProyecto(_ana);

            var resultado = ProyectosComo(_beto).Edit(proyectoDeAna.Id);

            Assert.IsType<UnauthorizedResult>(resultado);
        }

        [Fact]
        public void DeleteConfirmado_ProyectoAjeno_DevuelveUnauthorizedYNoLoElimina()
        {
            var proyectoDeAna = _db.CrearProyecto(_ana);

            var resultado = ProyectosComo(_beto).DeleteConfirmado(proyectoDeAna.Id);

            Assert.IsType<UnauthorizedResult>(resultado);
            Assert.True(_db.NuevoContexto().Proyectos.Any(p => p.Id == proyectoDeAna.Id));
        }

        [Fact]
        public async Task GetProyectosVencidos_SoloDevuelveLosDelUsuarioAutenticado()
        {
            _db.CrearProyecto(_ana, "Vencido");
            _db.CrearProyecto(_beto, "Vencido");

            var resultado = await ProyectosComo(_ana).GetProyectosVencidos();

            var ok = Assert.IsType<OkObjectResult>(resultado);
            var vencidos = Assert.IsType<List<Proyectos>>(ok.Value);
            var proyecto = Assert.Single(vencidos);
            Assert.Equal(_ana.Id, proyecto.UsuarioId);
        }

        // ---------- Notas ----------

        [Fact]
        public void Edit_NotaAjena_DevuelveUnauthorized()
        {
            var notaDeAna = _db.CrearNota(_ana);

            var resultado = NotasComo(_beto).Edit(notaDeAna.Id);

            Assert.IsType<UnauthorizedResult>(resultado);
        }

        [Fact]
        public void DeleteConfirmado_NotaAjena_DevuelveUnauthorizedYNoLaElimina()
        {
            var notaDeAna = _db.CrearNota(_ana);

            var resultado = NotasComo(_beto).DeleteConfirmado(notaDeAna.Id);

            Assert.IsType<UnauthorizedResult>(resultado);
            Assert.True(_db.NuevoContexto().Notas.Any(n => n.Id == notaDeAna.Id));
        }

        // ---------- Calendario ----------

        [Fact]
        public void EditarEvento_EventoAjeno_DevuelveUnauthorizedYNoLoModifica()
        {
            var eventoDeAna = _db.CrearEvento(_ana, "Examen");
            var cambios = new Calendario { Id = eventoDeAna.Id, Titulo = "Modificado por otro usuario" };

            var resultado = CalendariosComo(_beto).EditarEvento(cambios);

            Assert.IsType<UnauthorizedObjectResult>(resultado);
            var guardado = _db.NuevoContexto().Calendario.Single(e => e.Id == eventoDeAna.Id);
            Assert.Equal("Examen", guardado.Titulo);
        }

        [Fact]
        public void EliminarEvento_EventoAjeno_DevuelveUnauthorizedYNoLoElimina()
        {
            var eventoDeAna = _db.CrearEvento(_ana);

            var resultado = CalendariosComo(_beto).EliminarEvento(eventoDeAna.Id);

            Assert.IsType<UnauthorizedObjectResult>(resultado);
            Assert.True(_db.NuevoContexto().Calendario.Any(e => e.Id == eventoDeAna.Id));
        }
    }
}
