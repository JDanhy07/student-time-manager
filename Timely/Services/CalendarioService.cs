using Timely.Data;
using Timely.Models;
using Timely.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Timely.Services
{
    public class CalendarioService : ICalendarioService
    {
        private readonly ApplicationDbContext _context;

        public CalendarioService(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Calendario> ObtenerEventosPorUsuario(int usuarioId)
        {
            return _context.Calendario
                           .Where(e => e.UsuarioId == usuarioId)
                           .ToList();
        }

        public Calendario ObtenerEventoPorId(int id)
        {
            return _context.Calendario.FirstOrDefault(e => e.Id == id);
        }

        public void AgregarEvento(Calendario evento)
        {
            _context.Calendario.Add(evento);
            _context.SaveChanges();
        }

        public void EditarEvento(Calendario evento)
        {
            var existente = _context.Calendario.FirstOrDefault(e => e.Id == evento.Id)
                ?? throw new Exception($"No se encontró el evento con ID {evento.Id}.");

            existente.Titulo = evento.Titulo;
            existente.FechaInicio = evento.FechaInicio;
            existente.FechaFinal = evento.FechaFinal;
            existente.Descripcion = evento.Descripcion;

            _context.SaveChanges();
        }

        public void EliminarEvento(int id)
        {
            var evento = _context.Calendario.FirstOrDefault(e => e.Id == id)
                ?? throw new Exception($"No se encontró el evento con ID {id}.");

            _context.Calendario.Remove(evento);
            _context.SaveChanges();
        }
    }
}