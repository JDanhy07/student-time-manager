using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Timely.Controllers;

namespace Timely.Tests.Controllers
{
    // Verifican la configuración de seguridad de los controllers (atributos).
    // Protegen contra regresiones como la de los POST de administración sin [Authorize].
    public class AutorizacionTests
    {
        private const BindingFlags AccionesPublicas =
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        private static MethodInfo ObtenerAccion(string nombre, bool esPost) =>
            typeof(UsuariosController)
                .GetMethods(AccionesPublicas)
                .Single(m => m.Name == nombre
                          && (m.GetCustomAttribute<HttpPostAttribute>() != null) == esPost);

        [Theory]
        [InlineData("Lista", false)]
        [InlineData("Edit", false)]
        [InlineData("Edit", true)]
        [InlineData("Delete", false)]
        [InlineData("DeleteConfirmado", true)]
        [InlineData("CreateLista", false)]
        [InlineData("CreateLista", true)]
        public void AccionesDeAdministracion_ExigenElRolAdministrador(string nombre, bool esPost)
        {
            var accion = ObtenerAccion(nombre, esPost);

            var autorizaciones = accion.GetCustomAttributes<AuthorizeAttribute>();

            Assert.Contains(autorizaciones, a => a.Roles == "Administrador");
        }

        [Theory]
        [InlineData(typeof(UsuariosController))]
        [InlineData(typeof(ProyectosController))]
        [InlineData(typeof(NotasController))]
        [InlineData(typeof(CalendariosController))]
        public void TodosLosPostDeLosControllersMvc_ExigenTokenAntiforgery(Type controller)
        {
            var postsSinToken = controller
                .GetMethods(AccionesPublicas)
                .Where(m => m.GetCustomAttribute<HttpPostAttribute>() != null)
                .Where(m => m.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>() == null)
                .Select(m => m.Name)
                .ToList();

            Assert.Empty(postsSinToken);
        }
    }
}
