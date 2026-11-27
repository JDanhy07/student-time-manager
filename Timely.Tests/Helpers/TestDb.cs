using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Timely.Data;
using Timely.Models;

namespace Timely.Tests.Helpers
{
    // Base de datos SQLite en memoria, nueva para cada prueba.
    // Usa el mismo ApplicationDbContext de la aplicación, así que el modelo
    // (índices únicos, campos requeridos, etc.) es el real; solo cambia el proveedor.
    public class TestDb : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public ApplicationDbContext Context { get; }

        public TestDb()
        {
            // La BD en memoria vive mientras la conexión esté abierta
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;

            Context = new ApplicationDbContext(_options);
            Context.Database.EnsureCreated();
        }

        // Segundo contexto sobre la misma BD: sirve para comprobar lo que realmente
        // se guardó, sin depender de las entidades que el primer contexto ya tiene en memoria.
        public ApplicationDbContext NuevoContexto() => new ApplicationDbContext(_options);

        public Usuarios CrearUsuario(string nombre, string rol = "Estudiante")
        {
            var usuario = new Usuarios
            {
                Usuario = nombre,
                Correo = $"{nombre}@test.com",
                Contrasena = "hash-de-prueba",
                Rol = rol,
                Fecha = "2000-01-01"
            };
            Context.Usuarios.Add(usuario);
            Context.SaveChanges();
            return usuario;
        }

        public Proyectos CrearProyecto(Usuarios dueno, string estado = "En proceso")
        {
            var proyecto = new Proyectos
            {
                Nombre = "Proyecto de prueba",
                Fecha_de_inicio = DateTime.Now.AddDays(-10),
                Vence = DateTime.Now.AddDays(estado == "Vencido" ? -1 : 5),
                Estado = estado,
                UsuarioId = dueno.Id
            };
            Context.Proyectos.Add(proyecto);
            Context.SaveChanges();
            return proyecto;
        }

        public Nota CrearNota(Usuarios dueno, string titulo = "Nota de prueba")
        {
            var nota = new Nota
            {
                Titulo = titulo,
                Contenido = "Contenido",
                Carpeta = "General",
                UsuarioId = dueno.Id
            };
            Context.Notas.Add(nota);
            Context.SaveChanges();
            return nota;
        }

        public Calendario CrearEvento(Usuarios dueno, string titulo = "Evento de prueba")
        {
            var evento = new Calendario
            {
                Titulo = titulo,
                FechaInicio = "2026-11-20",
                FechaFinal = "2026-11-21",
                Descripcion = "Descripción",
                UsuarioId = dueno.Id
            };
            Context.Calendario.Add(evento);
            Context.SaveChanges();
            return evento;
        }

        public void Dispose()
        {
            Context.Dispose();
            _connection.Dispose();
        }
    }
}
