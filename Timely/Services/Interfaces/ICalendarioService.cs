using Timely.Models;

namespace Timely.Services.Interfaces
{
    public interface ICalendarioService
    {
        List<Calendario> ObtenerEventos();
        void AgregarEvento(Calendario evento);
        void EditarEvento(Calendario evento);
        void EliminarEvento(int id);
    }
}
