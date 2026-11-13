using Microsoft.EntityFrameworkCore;
using Timely.Models;
using Timely.Tests.Helpers;

namespace Timely.Tests.Data
{
    // Comprueban que los índices únicos definidos en el modelo Usuarios
    // se aplican en la base de datos (no solo en la validación de UsuarioService).
    public class IndicesUnicosTests : IDisposable
    {
        private readonly TestDb _db = new TestDb();

        public void Dispose() => _db.Dispose();

        private static Usuarios NuevoUsuario(string nombre, string correo) => new Usuarios
        {
            Usuario = nombre,
            Correo = correo,
            Contrasena = "hash-de-prueba",
            Rol = "Estudiante",
            Fecha = "2000-01-01"
        };

        [Fact]
        public void BaseDeDatos_RechazaDosUsuariosConElMismoNombre()
        {
            _db.CrearUsuario("ana");
            _db.Context.Usuarios.Add(NuevoUsuario("ana", "otro@test.com"));

            Assert.Throws<DbUpdateException>(() => _db.Context.SaveChanges());
        }

        [Fact]
        public void BaseDeDatos_RechazaDosUsuariosConElMismoCorreo()
        {
            _db.CrearUsuario("ana");
            _db.Context.Usuarios.Add(NuevoUsuario("beto", "ana@test.com"));

            Assert.Throws<DbUpdateException>(() => _db.Context.SaveChanges());
        }
    }
}
