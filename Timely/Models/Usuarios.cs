using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Timely.Models
{
    [Index(nameof(Usuario), IsUnique = true)] // Asegura que el nombre de usuario sea único
    [Index(nameof(Correo), IsUnique = true)] // Asegura que el correo electrónico sea único

    public class Usuarios
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El usuario es obligatorio")]
        [StringLength(50)]
        public string Usuario { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string Contrasena { get; set; }

        public string? Perfil { get; set; } //Ahora es solo informativo/preferencias, no se usa para control de acceso.

        [Required]
        public string Rol { get; set; } = "Estudiante"; // Valor predeterminado

        [Required]
        [EmailAddress]
        [StringLength(100)] 
        public string Correo { get; set; }

        [Required (ErrorMessage = "La fecha es obligatoria")]
        public string Fecha { get; set; }

        [NotMapped]
        [Compare("Contrasena", ErrorMessage = "Las contraseñas no coinciden")]
        public string Confirmacion { get; set; }
    }
}