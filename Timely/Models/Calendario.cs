using System.ComponentModel.DataAnnotations;

namespace Timely.Models
{
	public class Calendario
	{
		// Campos privados
		private int id;
		private string titulo;
		private string fechaInicio;
		private string fechaFinal;
		private string descripcion;

		// Constructor con parámetros para inicializar el objeto
		public Calendario(int id, string titulo, string fechaInicio, string fechaFinal, string descripcion)
		{
			this.id = id;
			this.titulo = titulo;
			this.fechaInicio = fechaInicio;
			this.fechaFinal = fechaFinal;
			this.descripcion = descripcion;
		}

		// Constructor por defecto (valores vacíos o cero)
		public Calendario()
		{
			this.Id = 0;
			this.Titulo = "";
			this.FechaInicio = "";
			this.FechaFinal = "";
			this.Descripcion = "";
		}

		// ID único del evento en el calendario
		[Key]
		public int Id { get => id; set => id = value; }

		// Título del evento, campo requerido
		[Required]
		public string Titulo { get => titulo; set => titulo = value; }

		// Fecha de inicio del evento (se espera tipo Date pero se usa string)
		[DataType(DataType.Date)]
		public string FechaInicio { get => fechaInicio; set => fechaInicio = value; }

		// Fecha final del evento
		[DataType(DataType.Date)]
		public string FechaFinal { get => fechaFinal; set => fechaFinal = value; }

		// Descripción adicional del evento
		public string Descripcion { get => descripcion; set => descripcion = value; }
	}
}
