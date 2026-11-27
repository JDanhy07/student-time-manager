namespace Timely.Models
{
    public class PaginaUsuarios
    {
        public List<Usuarios> Items { get; set; } = new();

        //Usuarios que cumplen el filtro (todas las páginas)
        public int Total { get; set; }

        //Página realmente devuelta (se ajusta si la pedida queda fuera de rango)
        public int Pagina { get; set; } = 1;

        public int TotalPaginas { get; set; } = 1;
    }
}