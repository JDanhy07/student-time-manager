using Timely.Data;
using Timely.Models;
using Timely.Services.Interfaces;

namespace Timely.Services
{
    public class NotaService : INotaService
    {
        private readonly ApplicationDbContext _context;

        public NotaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void AgregarNota(Nota nota)
        {
            _context.Notas.Add(nota);
            _context.SaveChanges();
        }

        public List<Nota> ObtenerTodas()
        {
            return _context.Notas.ToList();
        }

        public Nota BuscarPorId(int id)
        {
            return _context.Notas.FirstOrDefault(n => n.Id == id)
                ?? throw new Exception($"No se encontró la nota con ID {id}.");
        }

        public void ActualizarNota(Nota nota)
        {
            var existente = BuscarPorId(nota.Id);
            existente.Contenido = nota.Contenido;
            existente.Carpeta = nota.Carpeta;
            _context.SaveChanges();
        }

        public void EliminarNota(Nota nota)
        {
            _context.Notas.Remove(nota);
            _context.SaveChanges();
        }
    }
}
