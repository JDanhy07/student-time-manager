using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Permissions;

namespace Timely.Models
{
	// Clase que representa a un Usuario en la aplicación
	public class Usuarios
	{
		// Propiedades privadas
		private int id;
		private string usuario;
		private int contrasena;
		private string perfil;
		private string correo;
		private int confirmacion;
		private string fecha;

		// Constructor con parámetros que inicializa un nuevo usuario con los valores proporcionados
		public Usuarios(int id, string usuario, int contrasena, string perfil, string correo, int confirmacion, string fecha)
		{
			this.Id = id;
			this.Usuario = usuario;
			this.Contrasena = contrasena;
			this.Perfil = perfil;
			this.Correo = correo;
			this.Confirmacion = confirmacion;
			this.Fecha = fecha;
		}

		// Constructor sin parámetros que inicializa un usuario con valores por defecto
		public Usuarios()
		{
			this.Id = 0;
			this.Usuario = "";
			this.Contrasena = 0;
			this.Perfil = "";
			this.Correo = "";
			this.Confirmacion = 0;
			this.Fecha = "";
		}

		// Propiedad pública Id para acceder y modificar el id del usuario
		[Key] // Indica que esta propiedad es la clave primaria en la base de datos
		public int Id { get => id; set => id = value; }

		// Propiedad pública Usuario para acceder y modificar el nombre de usuario
		public string Usuario { get => usuario; set => usuario = value; }

		// Propiedad pública Contrasena con validación requerida (se espera que el valor sea un número)
		[Required] // Indica que esta propiedad es obligatoria
		public int Contrasena { get => contrasena; set => contrasena = value; }

		// Propiedad pública Perfil para acceder y modificar el perfil del usuario (ej. "Admin", "Usuario", etc.)
		public string Perfil { get => perfil; set => perfil = value; }

		// Propiedad pública Correo para acceder y modificar el correo electrónico del usuario
		public string Correo { get => correo; set => correo = value; }

		// Propiedad pública Fecha para acceder y modificar la fecha de creación de la cuenta del usuario
		public string Fecha { get => fecha; set => fecha = value; }

		// Propiedad pública Confirmacion que valida que la contraseña y la confirmación coinciden
		[Compare("Contrasena", ErrorMessage = "Las contrasenas no coinciden")] // Valida que las contraseñas coincidan
		[NotMapped] // Esta propiedad no se mapea a la base de datos, solo se usa para comparación
		public int Confirmacion { get => confirmacion; set => confirmacion = value; }
	}
}
