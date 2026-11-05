using System;
using System.Collections.Generic;

namespace Timely.Models
{
    public class HogarDashboardViewModel
    {
        public string NombreUsuario { get; set; } = "";
        public string Saludo { get; set; } = "";
        public string FechaHoy { get; set; } = "";

        public int EventosHoy { get; set; }
        public int EventosSemana { get; set; }

        public List<EventoResumen> ProximosEventos { get; set; } = new();
    }

    public class EventoResumen
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public DateTime Inicio { get; set; }
        public DateTime? Fin { get; set; }
    }
}