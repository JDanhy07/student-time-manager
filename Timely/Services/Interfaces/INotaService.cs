using Timely.Models;

namespace Timely.Services.Interfaces
{
    public interface INotaService
    {
        void AgregarNota(Nota nota);
        List<Nota> ObtenerTodas();
        Nota BuscarPorId(int id);
        void ActualizarNota(Nota nota);
        void EliminarNota(Nota nota);
    }
}
