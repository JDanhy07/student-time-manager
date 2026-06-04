using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Timely.Models
{
    public class Usuarios
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El usuario es obligatorio")]
        [StringLength(50)]
        public string Usuario { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public int Contrasena { get; set; }

        [Required]
        public string Perfil { get; set; }

        [Required]
        [EmailAddress]
        public string Correo { get; set; }

        [Required (ErrorMessage = "La contraseña es obligatoria")]
        public string Fecha { get; set; }

        [NotMapped]
        [Compare("Contrasena", ErrorMessage = "Las contraseñas no coinciden")]
        public int Confirmacion { get; set; }
    }
}