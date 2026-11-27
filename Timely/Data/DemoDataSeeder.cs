using System.Globalization;
using Timely.Models;
using Timely.Services;

namespace Timely.Data
{
    // Carga datos de demostración para poder probar la aplicación sin registrar nada a mano.
    // Usa los mismos servicios que la aplicación, así las contraseñas se guardan con hash
    // y se aplican las mismas validaciones (usuario y correo únicos).
    // Las credenciales son SOLO de demostración: Program.cs únicamente las siembra en desarrollo.
    public static class DemoDataSeeder
    {
        public const string UsuarioAdmin = "admin";
        public const string ContrasenaAdmin = "Admin123";

        public const string UsuarioEstudiante = "estudiante";
        public const string ContrasenaEstudiante = "Estudiante123";

        public static void Sembrar(ApplicationDbContext context)
        {
            // Solo se siembra en una base de datos vacía: nunca se mezcla con datos reales
            if (context.Usuarios.Any()) return;

            var usuarioService = new UsuarioService(context);
            var proyectoService = new ProyectoService(context);
            var notaService = new NotaService(context);
            var calendarioService = new CalendarioService(context);

            usuarioService.AgregarUsuario(new Usuarios
            {
                Usuario = UsuarioAdmin,
                Correo = "admin@timely.demo",
                Contrasena = ContrasenaAdmin,
                Rol = "Administrador",
                Fecha = "1990-01-01"
            });

            var estudiante = new Usuarios
            {
                Usuario = UsuarioEstudiante,
                Correo = "estudiante@timely.demo",
                Contrasena = ContrasenaEstudiante,
                Rol = "Estudiante",
                Fecha = "2002-05-15"
            };
            usuarioService.AgregarUsuario(estudiante); // EF completa el Id al guardar

            // Las fechas son relativas a hoy para que la demo siempre se vea actual.
            // Completado va al final: su setter calcula el Estado a partir de Vence.
            var hoy = DateTime.Today;

            proyectoService.AgregarProyecto(new Proyectos
            {
                Nombre = "Proyecto final de Programación",
                Fecha_de_inicio = hoy.AddDays(-14),
                Vence = hoy.AddDays(10),
                UsuarioId = estudiante.Id,
                Completado = false              // En proceso
            });
            proyectoService.AgregarProyecto(new Proyectos
            {
                Nombre = "Tarea de Bases de Datos",
                Fecha_de_inicio = hoy.AddDays(-20),
                Vence = hoy.AddDays(-2),
                UsuarioId = estudiante.Id,
                Completado = false              // Vencido
            });
            proyectoService.AgregarProyecto(new Proyectos
            {
                Nombre = "Informe de laboratorio de Física",
                Fecha_de_inicio = hoy.AddDays(-10),
                Vence = hoy.AddDays(3),
                UsuarioId = estudiante.Id,
                Completado = true               // Hecho
            });
            proyectoService.AgregarProyecto(new Proyectos
            {
                Nombre = "Preparar presentación de Ética",
                Fecha_de_inicio = hoy.AddDays(5),
                Vence = hoy.AddDays(20),
                UsuarioId = estudiante.Id,
                Completado = false              // Inactivo: aún no empieza
            });

            notaService.AgregarNota(new Nota
            {
                Titulo = "Temas del examen de Cálculo",
                Contenido = "Límites, derivadas y regla de la cadena. Repasar los ejercicios del capítulo 4.",
                Carpeta = "Universidad",
                UsuarioId = estudiante.Id
            });
            notaService.AgregarNota(new Nota
            {
                Titulo = "Ideas para el proyecto final",
                Contenido = "Agregar autenticación, un tablero de tareas y exportar los resultados a PDF.",
                Carpeta = "Universidad",
                UsuarioId = estudiante.Id
            });
            notaService.AgregarNota(new Nota
            {
                Titulo = "Lista de compras",
                Contenido = "Cuadernos, marcadores y una memoria USB.",
                Carpeta = "Personal",
                UsuarioId = estudiante.Id
            });

            calendarioService.AgregarEvento(CrearEvento(estudiante, "Reunión de grupo",
                hoy.AddDays(1).AddHours(15), hoy.AddDays(1).AddHours(16).AddMinutes(30),
                "Repartir tareas del proyecto final."));
            calendarioService.AgregarEvento(CrearEvento(estudiante, "Examen parcial de Cálculo",
                hoy.AddDays(3).AddHours(8), hoy.AddDays(3).AddHours(10),
                "Aula 204. Llevar calculadora."));
            calendarioService.AgregarEvento(CrearEvento(estudiante, "Tutoría de Bases de Datos",
                hoy.AddDays(6).AddHours(10), hoy.AddDays(6).AddHours(11),
                "Consultar dudas sobre normalización."));
            calendarioService.AgregarEvento(CrearEvento(estudiante, "Entrega del proyecto final",
                hoy.AddDays(10).AddHours(23).AddMinutes(59), hoy.AddDays(10).AddHours(23).AddMinutes(59),
                "Subir el código y el informe a la plataforma."));
        }

        // Mismo formato que envía el formulario del calendario (datetime-local)
        private static Calendario CrearEvento(Usuarios usuario, string titulo, DateTime inicio, DateTime fin, string descripcion)
        {
            const string formato = "yyyy-MM-ddTHH:mm";

            return new Calendario
            {
                Titulo = titulo,
                FechaInicio = inicio.ToString(formato, CultureInfo.InvariantCulture),
                FechaFinal = fin.ToString(formato, CultureInfo.InvariantCulture),
                Descripcion = descripcion,
                UsuarioId = usuario.Id
            };
        }
    }
}
