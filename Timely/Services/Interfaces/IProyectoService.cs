using Timely.Models;

namespace Timely.Services.Interfaces
{
    public interface IProyectoService
    {
        void AgregarProyecto(Proyectos proyecto);
        List<Proyectos> ObtenerPorUsuario(int usuarioId);
        Proyectos BuscarPorId(int id);
        void ActualizarProyecto(Proyectos proyecto);
        void EliminarProyecto(Proyectos proyecto);
        Task<List<Proyectos>> ObtenerVencidosAsync();
        string ObtenerClaseEstado(DateTime fechaInicio, DateTime fechaVencimiento, string estado);
    }
}
