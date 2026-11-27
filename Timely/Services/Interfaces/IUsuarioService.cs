using System.Security.Claims;
using Timely.Models;

namespace Timely.Services.Interfaces
{
    public interface IUsuarioService
    {
        void AgregarUsuario(Usuarios usuario);
        List<Usuarios> ObtenerTodos();

        // Lista de administración: el filtro, el orden y la paginación se resuelven en la base de datos
        PaginaUsuarios Buscar(string? texto, string? rol, string? orden, int pagina, int tamanoPagina);
        Dictionary<string, int> ContarPorRol();
        Usuarios BuscarPorId(int id);
        void ActualizarUsuario(Usuarios usuario);
        void EliminarUsuario(Usuarios usuario);

        // TODO (Fase seguridad): cambiar int a string y aplicar BCrypt
        Usuarios Login(string nombreUsuario, string contrasena);
        Usuarios ObtenerPerfil(ClaimsPrincipal user);

        // Autoservicio: la persona edita SU propio perfil (nunca el rol ni la contraseña por esta vía)
        void ActualizarPerfil(int id, string usuario, string correo, string fecha);
        bool CambiarContrasena(int id, string contrasenaActual, string contrasenaNueva);
    }
}