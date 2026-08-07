
namespace Timely.Models
{
	public class Proyectos
	{

		public int Id { get; set; }

		public required string Nombre { get; set; }

		public DateTime Fecha_de_inicio { get; set; }

		public DateTime Vence { get; set; }

		public string Estado;

        private bool _completado;

		public bool Completado
		{
			// Retorna el valor actual
			get => _completado;

			// Al asignar un nuevo valor, actualiza automáticamente el estado
			set
			{
				_completado = value;

				// Si está completado, el estado es "Hecho"
				// Si no está completado y ya venció, el estado es "Vencido"
				// Si no está completado y aún está dentro del plazo, el estado es "En proceso"
				Estado = _completado ? "Hecho" : (DateTime.Now > Vence ? "Vencido" : "En proceso");
			}
		}

		// Clase anidada que sirve como DTO (Data Transfer Object) para actualizar el estado del proyecto
		public class EstadoUpdateDto
		{
			// Id del proyecto que se quiere actualizar
			public int Id { get; set; }

			// Nuevo valor para la propiedad 'Completado'
			public bool Completado { get; set; }
		}
	}
}
