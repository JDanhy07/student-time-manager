using Timely.Models;
using System.Collections.Generic;

namespace Timely.Services.Interfaces
{
    public interface ICalendarioService
    {
        List<Calendario> ObtenerEventosPorUsuario(int usuarioId);
        void AgregarEvento(Calendario evento);
        void EditarEvento(Calendario evento);
        void EliminarEvento(int id);
        Calendario ObtenerEventoPorId(int id); // Método para obtener un evento por su ID
    }
}
