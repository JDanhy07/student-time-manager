using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Timely.Models;

namespace Timely.Tests.Helpers
{
    public static class ControllerTestExtensions
    {
        // Simula una sesión iniciada con los mismos claims que crea UsuariosController.Login
        public static T ComoUsuario<T>(this T controller, Usuarios usuario) where T : Controller
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Usuario),
                new Claim(ClaimTypes.Role, usuario.Rol)
            };

            var identidad = new ClaimsIdentity(claims, "PruebasAuth");

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identidad) }
            };

            return controller;
        }
    }
}
