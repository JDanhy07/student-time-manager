using Timely.Models;

namespace Timely.Models
{
    //Datos de la pantalla "Lista de usuarios" (búsqueda, filtro, orden y paginación).
    public class ListaUsuariosViewModel
    {
        public const int TamanoPagina = 12;

        public List<Usuarios> Items { get; set; } = new();

        public string? Busqueda { get; set; }
        public string? Rol { get; set; }      // null/"" = todos
        public string Orden { get; set; } = "nombre";

        public int Pagina { get; set; } = 1;
        public int TotalPaginas { get; set; } = 1;

        //Resultados que cumplen los filtros (todas las páginas)
        public int TotalFiltrado { get; set; }

        // Conteos globales, para las pastillas de resumen
        public int Total { get; set; }
        public int TotalAdministradores { get; set; }
        public int TotalEstudiantes { get; set; }

        public bool HayFiltros => !string.IsNullOrWhiteSpace(Busqueda) || !string.IsNullOrWhiteSpace(Rol);
    }
}