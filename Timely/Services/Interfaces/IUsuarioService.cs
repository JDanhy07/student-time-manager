using System.Security.Claims;
using Timely.Models;

namespace Timely.Services.Interfaces
{
    public interface IUsuarioService
    {
        void AgregarUsuario(Usuarios usuario);
        List<Usuarios> ObtenerTodos();
        Usuarios BuscarPorId(int id);
        void ActualizarUsuario(Usuarios usuario);
        void EliminarUsuario(Usuarios usuario);

        // TODO (Fase seguridad): cambiar int a string y aplicar BCrypt
        Usuarios Login(string nombreUsuario, int contrasena);
        Usuarios ObtenerPerfil(ClaimsPrincipal user);
    }
}
