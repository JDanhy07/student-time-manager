using Microsoft.EntityFrameworkCore;
using Timely.Data;
using Timely.Models;
using Timely.Services.Interfaces;

namespace Timely.Services
{
    public class ProyectoService : IProyectoService
    {
        private readonly ApplicationDbContext _context;

        public ProyectoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void AgregarProyecto(Proyectos proyecto)
        {
            _context.Proyectos.Add(proyecto);
            _context.SaveChanges();
            Console.WriteLine($"Proyecto agregado: {proyecto.Nombre}, ID: {proyecto.Id}"); 
        }

        public List<Proyectos> ObtenerTodos()
        {
            return _context.Proyectos.ToList();
        }

        public Proyectos BuscarPorId(int id)
        {
            return _context.Proyectos.FirstOrDefault(p => p.Id == id)
                ?? throw new Exception($"No se encontró el proyecto con ID {id}.");
        }

        public void ActualizarProyecto(Proyectos proyecto)
        {
            var existente = BuscarPorId(proyecto.Id);
            existente.Nombre = proyecto.Nombre;
            existente.Fecha_de_inicio = proyecto.Fecha_de_inicio;
            existente.Vence = proyecto.Vence;
            existente.Estado = proyecto.Estado;
            _context.SaveChanges();
        }

        public void EliminarProyecto(Proyectos proyecto)
        {
            _context.Proyectos.Remove(proyecto);
            _context.SaveChanges();
        }

        // EF Core: ToListAsync() viene de Microsoft.EntityFrameworkCore, no de System.Data.Entity
        public async Task<List<Proyectos>> ObtenerVencidosAsync()
        {
            return await _context.Proyectos
                .Where(p => p.Estado == "Vencido")
                .ToListAsync();
        }

        public string ObtenerClaseEstado(DateTime fechaInicio, DateTime fechaVencimiento, string estado)
        {
            DateTime hoy = DateTime.Now;

            if (estado == "Hecho") return "estado-hecho";
            if (hoy < fechaInicio) return "estado-inactivo";
            if (hoy >= fechaInicio && hoy <= fechaVencimiento) return "estado-proceso";
            if (hoy > fechaVencimiento) return "estado-vencido";

            return "estado-normal";
        }
    }
}
