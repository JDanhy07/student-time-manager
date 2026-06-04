using System.Security.Claims;
using Timely.Data;
using Timely.Models;
using Timely.Services.Interfaces;

namespace Timely.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly ApplicationDbContext _context;

        public UsuarioService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void AgregarUsuario(Usuarios user)
        {
            _context.Usuarios.Add(user);
            _context.SaveChanges();
        }

        public List<Usuarios> ObtenerTodos()
        {
            return _context.Usuarios.ToList();
        }

        public Usuarios BuscarPorId(int id)
        {
            return _context.Usuarios.FirstOrDefault(u => u.Id == id)
                ?? throw new Exception($"No se encontró el usuario con ID {id}.");
        }

        public void ActualizarUsuario(Usuarios usuario)
        {
            var existente = BuscarPorId(usuario.Id);
            existente.Usuario = usuario.Usuario;
            existente.Contrasena = usuario.Contrasena;
            existente.Perfil = usuario.Perfil;
            existente.Correo = usuario.Correo;
            existente.Confirmacion = usuario.Confirmacion;
            existente.Fecha = usuario.Fecha;
            _context.SaveChanges();
        }

        public void EliminarUsuario(Usuarios usuario)
        {
            _context.Usuarios.Remove(usuario);
            _context.SaveChanges();
        }

        // TODO (Fase seguridad): cambiar contrasena a string y validar con BCrypt.Verify()
        public Usuarios Login(string nombreUsuario, int contrasena)
        {
            return _context.Usuarios
                .FirstOrDefault(u => u.Usuario == nombreUsuario && u.Contrasena == contrasena);
        }

        public Usuarios ObtenerPerfil(ClaimsPrincipal user)
        {
            var nombreUsuario = user.Identity?.Name;
            return _context.Usuarios.FirstOrDefault(u => u.Usuario == nombreUsuario);
        }
    }
}
