// Namespace donde está definido el modelo del proyecto
namespace Timely.Models
{
	// Clase que representa un proyecto dentro de la aplicación
	public class Proyectos
	{
		// Identificador único del proyecto (campo requerido)
		public required int Id { get; set; }

		// Nombre del proyecto (campo requerido)
		public required string Nombre { get; set; }

		// Fecha en que el proyecto comienza
		public DateTime Fecha_de_inicio { get; set; }

		// Fecha en que el proyecto vence o debe completarse
		public DateTime Vence { get; set; }

		// Estado textual del proyecto ("Hecho", "Vencido" o "En proceso")
		public string Estado;

		// Campo privado que almacena si el proyecto está completado
		private bool _completado;

		// Propiedad pública que expone y modifica el estado 'Completado'
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
