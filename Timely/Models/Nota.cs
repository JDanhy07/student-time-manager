// Namespace donde se agrupa el modelo Nota
namespace Timely.Models
{
	// Clase que representa una nota dentro de la aplicación
	public class Nota
	{
		// Identificador único de la nota (clave primaria si se guarda en una base de datos)
		public int Id { get; set; }

		// Título de la nota
		public string Titulo { get; set; }

		// Contenido o cuerpo del texto de la nota
		public string Contenido { get; set; }

		// Carpeta a la que pertenece la nota (permite agrupar u organizar notas por categoría)
		public string Carpeta { get; set; } // Nueva propiedad para organizar notas
	}
}
