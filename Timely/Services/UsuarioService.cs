using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Timely.Data;
using Timely.Models;
using Timely.Services.Interfaces;

namespace Timely.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<Usuarios> _passwordHasher; 

        public UsuarioService(ApplicationDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<Usuarios>();
        }

        public void AgregarUsuario(Usuarios user)
        {
            user.Contrasena = _passwordHasher.HashPassword(user, user.Contrasena);

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
            if (!string.IsNullOrEmpty(usuario.Contrasena) && usuario.Contrasena != existente.Contrasena)
            {
                existente.Contrasena = _passwordHasher.HashPassword(existente, usuario.Contrasena);
            }
            existente.Perfil = usuario.Perfil;
            existente.Correo = usuario.Correo;
            existente.Fecha = usuario.Fecha;
            _context.SaveChanges();
        }

        public void EliminarUsuario(Usuarios usuario)
        {
            _context.Usuarios.Remove(usuario);
            _context.SaveChanges();
        }

        // TODO (Fase seguridad): cambiar contrasena a string y validar con BCrypt.Verify()
        public Usuarios Login(string nombreUsuario, string contrasena)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Usuario == nombreUsuario);

            if (usuario == null)
            {
                return null; //usuario no existe
            }

            //Comparamos el HASH de la BD con la contrasena que digito el usuario
            var resultado = _passwordHasher.VerifyHashedPassword(usuario, usuario.Contrasena, contrasena);

            if (resultado == PasswordVerificationResult.Success)
            {
                return usuario;
            }

            return null; //Contrasena incorrecta

        }

        public Usuarios ObtenerPerfil(ClaimsPrincipal user)
        {
            var nombreUsuario = user.Identity?.Name;
            return _context.Usuarios.FirstOrDefault(u => u.Usuario == nombreUsuario);
        }
    }
}
