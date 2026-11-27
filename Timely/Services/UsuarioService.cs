using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

        public void ValidarUnicidad(Usuarios user)
        {
            if (_context.Usuarios.Any(u => u.Id != user.Id && u.Usuario == user.Usuario))
            {
                throw new InvalidOperationException("El nombre del usuario ya esta en uso. Por favor, elija otro nombre de usuario.");
            }

            if (_context.Usuarios.Any(u => u.Id != user.Id && u.Correo == user.Correo))
            {
                throw new InvalidOperationException("El correo electrónico ya esta en uso. Por favor, elija otro correo electrónico.");
            }
        }

        public void AgregarUsuario(Usuarios user)
        {
            ValidarUnicidad(user); //Llamamos al metodo para validar la unicidad antes de agregar el usuario
            user.Contrasena = _passwordHasher.HashPassword(user, user.Contrasena);

            _context.Usuarios.Add(user);
            _context.SaveChanges();
        }

        public List<Usuarios> ObtenerTodos()
        {
            return _context.Usuarios.ToList();
        }

        public PaginaUsuarios Buscar(string? texto, string? rol, string? orden, int pagina, int tamanoPagina)
        {
            if (tamanoPagina < 1) tamanoPagina = 1;

            IQueryable<Usuarios> consulta = _context.Usuarios.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(texto))
            {
                // LIKE con los comodines del texto escapados, para que "%" o "_" se busquen tal cual
                var patron = "%" + EscaparLike(texto.Trim()) + "%";
                consulta = consulta.Where(u =>
                    EF.Functions.Like(u.Usuario, patron, "\\") ||
                    EF.Functions.Like(u.Correo, patron, "\\"));
            }

            if (!string.IsNullOrWhiteSpace(rol))
            {
                consulta = consulta.Where(u => u.Rol == rol);
            }

            // El Id como segundo criterio mantiene estable el orden entre páginas
            consulta = orden switch
            {
                "recientes" => consulta.OrderByDescending(u => u.Id),
                "antiguos" => consulta.OrderBy(u => u.Id),
                _ => consulta.OrderBy(u => u.Usuario).ThenBy(u => u.Id)
            };

            var total = consulta.Count();
            var totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)tamanoPagina));
            pagina = Math.Min(Math.Max(1, pagina), totalPaginas);

            var items = consulta
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToList();

            return new PaginaUsuarios
            {
                Items = items,
                Total = total,
                Pagina = pagina,
                TotalPaginas = totalPaginas
            };
        }

        public Dictionary<string, int> ContarPorRol()
        {
            return _context.Usuarios
                .GroupBy(u => u.Rol)
                .Select(g => new { Rol = g.Key, Cantidad = g.Count() })
                .ToDictionary(x => x.Rol, x => x.Cantidad);
        }

        private static string EscaparLike(string texto)
        {
            return texto
                .Replace("\\", "\\\\")
                .Replace("%", "\\%")
                .Replace("_", "\\_")
                .Replace("[", "\\[");
        }

        public Usuarios BuscarPorId(int id)
        {
            return _context.Usuarios.FirstOrDefault(u => u.Id == id)
                ?? throw new Exception($"No se encontró el usuario con ID {id}.");
        }

        public void ActualizarUsuario(Usuarios usuario)
        {
            ValidarUnicidad(usuario); //Llamamos al metodo para validar la unicidad antes de actualizar el usuario
            var existente = BuscarPorId(usuario.Id);
            existente.Usuario = usuario.Usuario;
            if (!string.IsNullOrEmpty(usuario.Contrasena))
            {
                existente.Contrasena = _passwordHasher.HashPassword(existente, usuario.Contrasena);
            }
            existente.Rol = usuario.Rol;
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

        public void ActualizarPerfil(int id, string usuario, string correo, string fecha)
        {
            var existente = BuscarPorId(id);
            usuario = usuario.Trim();
            correo = correo.Trim();

            // Misma regla de unicidad que el resto del sitio, ignorando al propio usuario
            ValidarUnicidad(new Usuarios { Id = id, Usuario = usuario, Correo = correo });

            // Solo se tocan estos tres campos: el rol y la contraseña quedan intactos
            existente.Usuario = usuario;
            existente.Correo = correo;
            existente.Fecha = fecha;
            _context.SaveChanges();
        }

        public bool CambiarContrasena(int id, string contrasenaActual, string contrasenaNueva)
        {
            var existente = BuscarPorId(id);

            var resultado = _passwordHasher.VerifyHashedPassword(existente, existente.Contrasena, contrasenaActual);
            if (resultado == PasswordVerificationResult.Failed)
            {
                return false; // la contraseña actual no coincide
            }

            existente.Contrasena = _passwordHasher.HashPassword(existente, contrasenaNueva);
            _context.SaveChanges();
            return true;
        }

        public Usuarios ObtenerPerfil(ClaimsPrincipal user)
        {
            // Se prioriza el Id guardado en el claim al iniciar sesión
            if (int.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id))
            {
                return _context.Usuarios.FirstOrDefault(u => u.Id == id);
            }

            // Respaldo para cookies emitidas antes de agregar el claim (el nombre ahora es único)
            var nombreUsuario = user.Identity?.Name;
            return _context.Usuarios.FirstOrDefault(u => u.Usuario == nombreUsuario);
        }
    }
}